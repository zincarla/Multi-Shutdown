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
#if DEBUG
using System.IO;
#endif

namespace Multi_Shutdown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, MachineInfoControl> MachineControlPanels = new Dictionary<string, MachineInfoControl>();
        bool CommandRunning = false;
        public MainWindow()
        {
            InitializeComponent();
            Refresh();
        }

        #region Menu

        #region Options
        private void nodesButton_Click(object sender, RoutedEventArgs e)
        {
            MachinesWindow MW = new MachinesWindow();
            if (MW.ShowDialog().HasValue&&MW.DialogResult.Value)
            {
                Refresh();
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow SettingWindow = new SettingsWindow();
            if (SettingWindow.ShowDialog().HasValue && SettingWindow.DialogResult.Value)
            {
                Refresh();
            }
        }
        #endregion

        #region Shutdown menu
        private void shutdownAll_Click(object sender, RoutedEventArgs e)
        {
            if (!CommandRunning)
            {
                ShutdownComputers(ShutdownMode.All);
            }
            else
            {
                MessageBox.Show("A command is already in progress");
            }
        }

        private void shutdownCritical_Click(object sender, RoutedEventArgs e)
        {
            if (!CommandRunning)
            {
                ShutdownComputers(ShutdownMode.Critical);
            }
            else
            {
                MessageBox.Show("A command is already in progress");
            }
        }

        private void shutdownHigh_Click(object sender, RoutedEventArgs e)
        {
            if (!CommandRunning)
            {
                ShutdownComputers(ShutdownMode.High);
            }
            else
            {
                MessageBox.Show("A command is already in progress");
            }
        }

        private void shutdownNonCritical_Click(object sender, RoutedEventArgs e)
        {
            if (!CommandRunning)
            {
                ShutdownComputers(ShutdownMode.NonCritical);
            }
            else
            {
                MessageBox.Show("A command is already in progress");
            }
        }
        #endregion

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow ABW = new AboutWindow();
            ABW.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Title = "Remove background threads...";
            Clear();
            Close();
        }
        #endregion

        #region DebugLog
        #if DEBUG
        public static void WriteToLog(string Msg)
        {
            if (!Directory.Exists(@"C:\Debug\Multi-Shutdown\"))
            {
                Directory.CreateDirectory(@"C:\Debug\Multi-Shutdown\");
            }
            using (StreamWriter SW = new StreamWriter(new FileStream(@"C:\Debug\Multi-Shutdown\txt.txt", FileMode.Append)))
            {
                SW.WriteLine("[" + DateTime.Now.ToString() + "] " + Msg);
            }
        }
        #endif
        #endregion

        #region Refresh/Clear
        /// <summary>
        /// Calls clear, and then reloads all the panels and options
        /// </summary>
        private void Refresh()
        {
            Clear();
            string err = Options.LoadOptions();
            if (err != "")
            {
                MessageBox.Show("No options file found, using defaults. You can create and change the options file in Edit>Settings.");
            }
            foreach (MachineInfo MI in Options.Machines)
            {
                MachineInfoControl MIC = new MachineInfoControl(MI);
                if (MI.Category == MachineCategory.Critical)
                {
                    criticalPanel.Children.Add(MIC);
                }
                else if (MI.Category == MachineCategory.High)
                {
                    highPanel.Children.Add(MIC);
                }
                else if (MI.Category == MachineCategory.NonCritical)
                {
                    nonCriticalPanel.Children.Add(MIC);
                }
                MachineControlPanels.Add(MIC.MachineInfo.FriendlyName, MIC);
            }
        }

        /// <summary>
        /// Reset the MainWindow to a state similiar to the first run (no machineInfoPanels)
        /// </summary>
        private void Clear()
        {
            //Close threads
            foreach (MachineInfoControl MIC in MachineControlPanels.Values)
            {
                MIC.Dispose();
            }
            MachineControlPanels.Clear();
            //Clear panels
            nonCriticalPanel.Children.Clear();
            highPanel.Children.Clear();
            criticalPanel.Children.Clear();
        }
        #endregion

        #region Shutdown
        ShutdownMode Mode;
        List<MachineInfoControl> CommandedMachines;
        Dictionary<string, List<MachineInfoControl>> MachineTree;// = new Dictionary<MachineInfoControl, List<MachineInfoControl>>();
        bool ShutdownDepedents = false;
        private void ShutdownComputers(ShutdownMode mode)
        {
            if (CommandRunning)
            {
                throw new Exception("A command is already running");
            }
            CommandRunning = true;
            if (mode != ShutdownMode.All)
            {
                ShutdownDepedents = (MessageBox.Show("Do you want to shutdown all dependent computers from other categories as well?\r\n"+
                    "Example: If a non-critical computer(A) depends on a critical computer(B), and you shutdown all critical, automatically shutdown A then B.", "Shutdown dependents?", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
            }
            else
            {
                ShutdownDepedents = true;
            }
            Mode = mode;
            MachineTree = new Dictionary<string, List<MachineInfoControl>>();
            CommandedMachines = new List<MachineInfoControl>();
            //Build full machine tree
            #region BuildTree
            List<MachineInfoControl> PotRoots = new List<MachineInfoControl>();

            ///General Idea
            ///Run through every machine info panel
            ////If the machine is not listed as dependedon, then add to PotRoots
            ////If the machine is depenent on anything, then run through those
            /////If the machine is already listed as dependedon, then just add the machine as another dependent
            /////Else, remove the dependedon from PotRoots, and add it to the tree as a dependedon with its first dependent

            foreach (MachineInfoControl MIC in MachineControlPanels.Values)
            {
                if (MIC.MachineInfo.Active)
                {
                    if (!MachineTree.ContainsKey(MIC.MachineInfo.FriendlyName))//If MIC is not currenly depended on
                    {
                        PotRoots.Add(MIC);//Add to the potential root list (no dependency)
                    }
                    if (MIC.MachineInfo.GetDependentOnAsArray() != null)//if MIC is dependent on something
                    {
                        foreach (string st in MIC.MachineInfo.GetDependentOnAsArray())//Go through the list of what it is dependent on
                        {
                            if (MachineControlPanels.ContainsKey(st))//If the machine exists
                            {
                                if (MachineTree.ContainsKey(st))//if the machine is already in the tree as depended on
                                {
                                    MachineTree[st].Add(MIC);//Add the machine as another dependent
                                }
                                else//if the machine is not listed as depended on
                                {
                                    if (MachineControlPanels[st].MachineInfo.Active)
                                    {
                                        MachineTree.Add(st, new List<MachineInfoControl>() { MIC });//Add as depended on with MIC as it's first dependent
                                        if (PotRoots.Contains(MachineControlPanels[st]))//And remove from potroots if it is in there (no longer list it as no dependency)
                                        {
                                            PotRoots.Remove(MachineControlPanels[st]);//Remove
                                        }
                                    }
                                }
                            }
                            //else skip
                        }

                    }
                }
            }
            MachineTree.Add("", PotRoots);//Finally put all pot roots (no dependencies) under null
            #endregion
            ParseTree();//Start parsing through the tree
        }

        private void ParseTree()
        {
            //Run through the tree and shutdown what you can (adding an event as well) and add to commanded
            foreach (KeyValuePair<string, List<MachineInfoControl>> KVP in MachineTree)
            {
                if (KVP.Value == null||KVP.Value.Count == 0)
                {
                    if (KVP.Key != "" && !CommandedMachines.Contains(MachineControlPanels[KVP.Key]))
                    {
#if DEBUG
                        WriteToLog("Shutting down " + MachineControlPanels[KVP.Key].MachineInfo.FriendlyName);
#endif
                        CommandedMachines.Add(MachineControlPanels[KVP.Key]);
                        MachineControlPanels[KVP.Key].CommandCompleted += new CommandFinished(MIC_CommandCompleted);
                        MachineControlPanels[KVP.Key].Shutdown();

                    }
                }
                else
                {
                    foreach (MachineInfoControl MIC in KVP.Value)
                    {
                        if (!MachineTree.ContainsKey(MIC.MachineInfo.FriendlyName) && !CommandedMachines.Contains(MIC))
                        {
                            MIC.CommandCompleted += new CommandFinished(MIC_CommandCompleted);
                            CommandedMachines.Add(MIC);
#if DEBUG
                            WriteToLog("Shutting down " + MIC.MachineInfo.FriendlyName);
#endif
                            MIC.Shutdown();
                        }
                    }
                }
            }


            if (MachineTree.ContainsKey("") && MachineTree[""].Count == 0)
            {
                MachineTree.Remove("");
            }
            if (MachineTree.Keys.Count == 0)
            {
                //FINISHED!!!!
                CommandedMachines.Clear(); CommandedMachines = null;
                MachineTree.Clear(); MachineTree = null;
                CommandRunning = false;
#if DEBUG
                WriteToLog("Finished command");
#endif
            }
        }

        void MIC_CommandCompleted(CommandResult CR, MachineInfoControl Sender)
        {
            Sender.CommandCompleted -= new CommandFinished(MIC_CommandCompleted);
            //TODO check CR for anything else
            if (CR == CommandResult.Success)
            {
                foreach (KeyValuePair<string, List<MachineInfoControl>> KVP in MachineTree)
                {
                    if (KVP.Value.Contains(Sender))
                    {
                        KVP.Value.Remove(Sender);
                    }
                }
                if (MachineTree.ContainsKey(Sender.MachineInfo.FriendlyName))
                {
                    MachineTree[Sender.MachineInfo.FriendlyName].Clear();
                    MachineTree[Sender.MachineInfo.FriendlyName] = null;
                    MachineTree.Remove(Sender.MachineInfo.FriendlyName);
                }
            }
            else if (CR == CommandResult.Cancel)
            {
                //Cancel
            }
            else
            {
                CommandedMachines.Remove(Sender);//Try again?
            }
            ParseTree();
        }
        #endregion
    }
    public enum ShutdownMode { All, NonCritical, Critical, High }
}
