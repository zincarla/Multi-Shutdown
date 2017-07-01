using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using System.Diagnostics;

namespace Multi_Shutdown
{
    /// <summary>
    /// Interaction logic for machineInfoControl.xaml
    /// This custom control represents a single monitored machine.
    /// </summary>
    public partial class MachineInfoControl : UserControl
    {
        #region PingVars
        DispatcherTimer PingTimer = new DispatcherTimer();
        Ping PingSender = new Ping();
        int FailedPings = 0;
        public const int MaxPingFails = 5;
        #endregion
        public MachineInfo MachineInfo { get; private set; }
        bool expectingDisconnect = false;
        DateTime disconnectTime;
        public event CommandFinished CommandCompleted;

        bool ExpectingDisconnect
        {
            get { return expectingDisconnect; }
            set
            {
                expectingDisconnect = value;
                if (expectingDisconnect)
                {
                    commandLight.State = LightState.Yellow;
                    PingTimer.Interval = TimeSpan.FromSeconds(5);
                }
                else
                {
                    commandLight.State = LightState.Off;
                    PingTimer.Interval = TimeSpan.FromSeconds(Options.PingInterval);
                }
            }
        }

        public MachineInfoControl(MachineInfo machineInfo)
        {
            InitializeComponent();
            MachineInfo = machineInfo;
            friendlyServerName.Content = machineInfo.FriendlyName;
            friendlyServerName.ToolTip = machineInfo.FQDN;
            PingTimer.Interval = TimeSpan.FromSeconds(Options.PingInterval);
            PingTimer.Tick += new EventHandler(PingTimer_Tick);
            PingSender.PingCompleted += new PingCompletedEventHandler(PingSender_PingCompleted);
            if (MachineInfo.Active)
            {
                PingSender.SendAsync(MachineInfo.FQDN, Options.PingTimeout);
            }
        }

        #region Ping
        void PingSender_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.Reply!=null&&e.Reply.Status != IPStatus.Success)
            {
                if (FailedPings == 0)
                {
                    //First fail
                    pingLight.State = LightState.Yellow;
                }
                FailedPings++;
                MainWindow.WriteToLog("Ping fail x"+FailedPings.ToString()+" for "+MachineInfo.FriendlyName+" with result "+e.Reply.Status.ToString());
                if (FailedPings >= MaxPingFails)
                {
                    FailedPings = 0;
                    pingLight.State = LightState.Red;
                    pingLight.ToolTip = "Ping Result: Off";
                    if (ExpectingDisconnect)
                    {
                        ExpectingDisconnect = false;
                        commandLight.State = LightState.Green;
                        if (CommandCompleted != null)
                        {
                            CommandCompleted(CommandResult.Success, this);
                        }
                    }
                    PingTimer.Start();
                }
                else
                {
                    pingLight.ToolTip = "Ping Result: Failing x" + FailedPings.ToString();
                    PingSender.SendAsync(MachineInfo.FQDN, Options.PingTimeout);
                }
            }
            else if (e.Reply!=null)
            {
                if (ExpectingDisconnect && DateTime.Now > disconnectTime)
                {
                    ExpectingDisconnect = false;
                    commandLight.State = LightState.Red;
                    if (CommandCompleted != null)
                    {
                        CommandCompleted(CommandResult.Fail, this);
                    }
                }
                FailedPings = 0;
                pingLight.State = LightState.Green;
                pingLight.BlinkLight = false;
                pingLight.ToolTip = "Ping Result: On";
                PingTimer.Start();
            }
        }

        void PingTimer_Tick(object sender, EventArgs e)
        {
            PingTimer.Stop();//Stop and wait for ping to finish
            PingSender.SendAsync(MachineInfo.FQDN, Options.PingTimeout);
        }
        #endregion

        public void Dispose()
        {
            PingTimer.Stop();
            PingSender.SendAsyncCancel();
        }

        #region Command Functions (Restart/shutdown/ect)
        public void Restart()
        {
            Process P = new Process();
            P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            P.StartInfo.FileName = "shutdown";
            P.StartInfo.Arguments = ("-m $FQDN -r -d p:5:19" +
                (Options.ForceRestart? " -f":"")+
                (" -t "+Options.DelayRestart.ToString())+
                (Options.ShowRestartMessage? " -c \""+Options.RestartMessage.Replace("\"", "")+"\"":"")
                ).Replace("$FQDN", MachineInfo.FQDN).Replace("$FNAME", MachineInfo.FriendlyName).Replace("$TIME", Options.DelayRestart.ToString()+"(s)");
            P.Start();
            ExpectingDisconnect = true;
            disconnectTime = DateTime.Now.AddSeconds(Options.DelayRestart + Options.CommandTimeout + 5 + (MaxPingFails * Options.PingTimeout / 1000));
        }

        public void Shutdown()
        {
            Process P = new Process();
            P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            P.StartInfo.FileName = "shutdown";
            P.StartInfo.Arguments = ("-m $FQDN -s -d p:5:19" +
                (Options.ForceShutdown ? " -f" : "") +
                (" -t " + Options.DelayShutdown.ToString()) +
                (Options.ShowShutdownMessage ? " -c \"" + Options.ShutdownMessage.Replace("\"", "") + "\"" : "")
                ).Replace("$FQDN", MachineInfo.FQDN).Replace("$FNAME", MachineInfo.FriendlyName).Replace("$TIME", Options.DelayShutdown.ToString() + "(s)");
            P.Start();
            ExpectingDisconnect = true;
            disconnectTime = DateTime.Now.AddSeconds(Options.DelayShutdown + Options.CommandTimeout + 5 + (MaxPingFails*Options.PingTimeout/1000));
        }

        public void Cancel()
        {
            Process P = new Process();
            P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            P.StartInfo.FileName = "shutdown";
            P.StartInfo.Arguments = "-a -m $FQDN".Replace("$FQDN", MachineInfo.FQDN);//.Replace("$FNAME", Machine.FriendlyName);
            P.Start();

            ExpectingDisconnect = false;
            commandLight.State = LightState.Off;
            if (CommandCompleted != null)
            {
                CommandCompleted(CommandResult.Cancel, this);
            }
        }
        #endregion

        #region Command Buttons
        private void shutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (Options.ShowConfirmation == false || MessageBox.Show("Are you sure you want to shutdown " + MachineInfo.FriendlyName + "(" + MachineInfo.FQDN + "?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Shutdown();
            }
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Options.ShowConfirmation == false || MessageBox.Show("Are you sure you want to restart "+MachineInfo.FriendlyName+"("+MachineInfo.FQDN+")?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Restart();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Options.ShowConfirmation == false || MessageBox.Show("Are you sure you want to cancel the shutdown/restart command?", "Are you sure?", MessageBoxButton.YesNo)== MessageBoxResult.Yes)
            {
                Cancel();
            }
        }
        #endregion

    }
    public delegate void CommandFinished(CommandResult CR, MachineInfoControl Sender);
    public enum CommandResult { Success, Fail, Cancel }
}
