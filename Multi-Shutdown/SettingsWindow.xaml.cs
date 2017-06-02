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
using System.Windows.Shapes;

namespace Multi_Shutdown
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            showConfirmCB.IsChecked = new bool?(Options.ShowConfirmation);
            commandTimeoutTB.Text = Options.CommandTimeout.ToString();

            pingIntervalTB.Text = Options.PingInterval.ToString();
            pingTimeoutTB.Text = Options.PingTimeout.ToString();

            forceRestartCB.IsChecked = new bool?(Options.ForceRestart);
            delayRestartTB.Text = Options.DelayRestart.ToString();
            showRestartMsgCB.IsChecked = new bool?(Options.ShowRestartMessage);
            restartMsgTB.Text = Options.RestartMessage;

            forceShutdownCB.IsChecked = new bool?(Options.ForceShutdown);
            delayShutdownTB.Text = Options.DelayShutdown.ToString();
            showShutdownMsgCB.IsChecked = new bool?(Options.ShowShutdownMessage);
            shutdownMsgTB.Text = Options.ShutdownMessage;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            int commandTimeout=600;
            int pingTimeout = 1000;
            int pingInterval = 10;
            int restartDelay = 60;
            int shutdownDelay = 60;
            if (int.TryParse(commandTimeoutTB.Text, out commandTimeout) && int.TryParse(pingTimeoutTB.Text, out pingTimeout) &&
                int.TryParse(pingIntervalTB.Text, out pingInterval) && int.TryParse(delayRestartTB.Text, out restartDelay) &&
                int.TryParse(delayShutdownTB.Text, out shutdownDelay))
            {
                Options.ShowConfirmation = (showConfirmCB.IsChecked.HasValue) ? showConfirmCB.IsChecked.Value : true;
                Options.CommandTimeout = commandTimeout;

                Options.PingInterval = pingInterval;
                Options.PingTimeout = pingTimeout;

                Options.ForceRestart = (forceRestartCB.IsChecked.HasValue) ? forceRestartCB.IsChecked.Value : true;
                Options.DelayRestart = restartDelay;
                Options.ShowRestartMessage = (showRestartMsgCB.IsChecked.HasValue) ? showRestartMsgCB.IsChecked.Value : true;
                Options.RestartMessage = restartMsgTB.Text;

                Options.ForceShutdown = (forceShutdownCB.IsChecked.HasValue) ? forceShutdownCB.IsChecked.Value : true;
                Options.DelayShutdown = shutdownDelay;
                Options.ShowShutdownMessage = (showShutdownMsgCB.IsChecked.HasValue) ? showShutdownMsgCB.IsChecked.Value : true;
                Options.ShutdownMessage = shutdownMsgTB.Text;

                Options.Save();
                DialogResult = new bool?(true);
                Close();
            }
            else
            {
                MessageBox.Show("Please check to make sure that all numerical values are numerical.");
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = new bool?(false);
            Close();
        }

        private void HelpMSG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(Options.HELPFormatMessage);
        }
    }
}
