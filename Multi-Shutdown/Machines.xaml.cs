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
    /// Interaction logic for Machines.xaml
    /// </summary>
    public partial class MachinesWindow : Window
    {
        public MachinesWindow()
        {
            InitializeComponent();
            List<MachineInfo> Source = new List<MachineInfo>();
            foreach (MachineInfo MI in Options.Machines)
            {
                Source.Add(MI.Clone());
            }
            if (Source.Count == 0)
            {
                Source.Add(new MachineInfo());
            }
            dataGrid.ItemsSource = Source;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool NoDups = true;
            List<string> FNCheck = new List<string>();
            bool ShowedFQDNMsg = false;
            foreach (MachineInfo MI in dataGrid.ItemsSource)
            {
                if (FNCheck.Contains(MI.FriendlyName))
                {
                    NoDups = false;
                    break;
                }
                else
                {
                    FNCheck.Add(MI.FriendlyName);
                }
                if (string.IsNullOrEmpty(MI.FQDN)&&!ShowedFQDNMsg)
                {
                    MessageBox.Show("One or more machines had their FQDN set to nothing. This will not harm the program, however, the ping will always fail.");
                    ShowedFQDNMsg = true;
                }
            }

            if (NoDups)
            {
                Options.Machines.Clear();
                foreach (MachineInfo MI in dataGrid.ItemsSource)
                {
                    if (!string.IsNullOrWhiteSpace(MI.FriendlyName) || !string.IsNullOrWhiteSpace(MI.FQDN) || !string.IsNullOrWhiteSpace(MI.DependentOn))
                    {
                        Options.Machines.Add(MI);
                    }
                }
                Options.Save();
                DialogResult = new bool?(true);
                Close();
            }
            else
            {
                MessageBox.Show("Please make sure all friendly names are unique.");
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = new bool?(false);
            Close();
        }
    }
}
