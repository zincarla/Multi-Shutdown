using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Multi_Shutdown
{
    static public class Options
    {
        static List<MachineInfo> machines = new List<MachineInfo>();
        public static List<MachineInfo> Machines { get { return machines; } private set { machines = value; } }
        public static bool ShowConfirmation = true;
        public static int CommandTimeout = 600;
        public const int MinCommandTimeout = 60;
        //Ping Settings
        public static int PingInterval = 5;//Seconds
        public static int PingTimeout = 1000; //Milliseconds
        public const int MinPingInterval = 1;
        public const int MinPingTimeout = 100;
        //Restart Settings
        public static bool ForceRestart = false;
        public static int DelayRestart = 60;//Seconds
        public const int MinDelayRestart = 0;
        public static string RestartMessage = "";
        public static bool ShowRestartMessage = true;
        //Shutdown Settings
        public static bool ForceShutdown = false;
        public static int DelayShutdown = 60;//Seconds
        public const int MinDelayShutdown = 0;
        public static string ShutdownMessage = "";
        public static bool ShowShutdownMessage = true;

        //Help messages
        public const string HELPFormatMessage = "The message attribute in the shutdown and restart category is a message that is shown to anyone currently signed into the target computer. Notes:\r\n\t*$FQDN will be replaced with the target machine's FQDN/IP\r\n\t*$FNAME will be replaced with the machine's friendly name\r\n\t*$TIME is replaced with the delay to restart/shutdown\r\n\t*All qutoes (\") will be automatically stripped."+
            "\r\nExample:\r\n    \"Your computer, \"$FNAME\"($FQDN) is restarting in $TIME.\" Becomes:\r\n    \"Your computer, Main SQL Server(192.168.1.200) is restarting in 60(s).\"";
        public static string Directory
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
        } 

        public static string LoadOptions()
        {
            try
            {
                List<string> sectiontree = new List<string>();
                Machines.Clear();
                if (File.Exists(Directory + "\\settings.xml"))
                {
                    XmlTextReader XReader = new XmlTextReader(Directory + "\\settings.xml");
                    MachineInfo Machine = new MachineInfo();
                    bool MachineFail = false;
                    bool ShowedHelp = false;
                    while (XReader.Read())
                    {
                        if (XReader.NodeType == XmlNodeType.Element)
                        {
                            sectiontree.Add(XReader.Name);
                            if (ToPath(sectiontree) == "\\Shutdown-Manager\\Settings\\ShutdownMessage")
                            {
                                ShowShutdownMessage = (XReader.GetAttribute("Show") == Boolean.FalseString) ? false : true;
                            }
                            if (ToPath(sectiontree) == "\\Shutdown-Manager\\Settings\\RestartMessage")
                            {
                                ShowRestartMessage = (XReader.GetAttribute("Show") == Boolean.FalseString) ? false : true;
                            }
                            if (ToPath(sectiontree) == "\\Shutdown-Manager\\Machines\\Machine")
                            {
                                Machine = new MachineInfo();
                            }
                        }
                        else if (XReader.NodeType == XmlNodeType.Text)
                        {
                            string path = ToPath(sectiontree);
                            if (path == "\\Shutdown-Manager\\Settings\\ShowConfirmation")
                            {
                                ShowConfirmation = (XReader.Value == Boolean.FalseString) ? false : true;
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\CommandTimeout")
                            {
                                int.TryParse(XReader.Value, out CommandTimeout);
                                if (CommandTimeout <= MinCommandTimeout)
                                {
                                    CommandTimeout = MinCommandTimeout;
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\PingInterval")
                            {
                                int.TryParse(XReader.Value, out PingInterval);
                                if (PingInterval <= MinPingInterval)
                                {
                                    PingInterval = MinPingInterval;
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\PingTimeout")
                            {
                                int.TryParse(XReader.Value, out PingTimeout);
                                if (PingTimeout <= MinPingTimeout)
                                {
                                    PingTimeout = MinPingTimeout;
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\ForceRestart")
                            {
                                ForceRestart = (XReader.Value == Boolean.FalseString) ? false : true;
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\DelayRestart")
                            {
                                int.TryParse(XReader.Value, out DelayRestart);
                                if (DelayRestart <= MinDelayRestart)
                                {
                                    DelayRestart = MinDelayRestart;
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\RestartMessage")
                            {
                                RestartMessage = XReader.Value;
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\ForceShutdown")
                            {
                                ForceShutdown = (XReader.Value == Boolean.FalseString) ? false : true;
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\DelayShutdown")
                            {
                                int.TryParse(XReader.Value, out DelayShutdown);
                                if (DelayShutdown <= MinDelayShutdown)
                                {
                                    DelayShutdown = MinDelayShutdown;
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Settings\\ShutdownMessage")
                            {
                                ShutdownMessage = XReader.Value;
                            }
                            //MachineInfo
                            else if (path == "\\Shutdown-Manager\\Machines\\Machine\\Name")
                            {
                                Machine.FQDN = XReader.Value;
                            }
                            else if (path == "\\Shutdown-Manager\\Machines\\Machine\\FriendlyName")
                            {
                                Machine.FriendlyName = XReader.Value;
                                foreach (MachineInfo MI in Machines)
                                {
                                    if (MI.FriendlyName == Machine.FriendlyName)
                                    {
                                        MachineFail = true;
                                        if (!ShowedHelp)
                                        {
                                            MessageBox.Show("A machine has a duplicate name or non existant category field. It has been removed or put into NonCritical category.");
                                        }
                                        ShowedHelp = true;
                                        break;
                                    }
                                }
                            }
                            else if (path == "\\Shutdown-Manager\\Machines\\Machine\\Dependency")
                            {
                                Machine.DependentOn = XReader.Value;
                            }
                            else if (path == "\\Shutdown-Manager\\Machines\\Machine\\Active")
                            {
                                Machine.Active = (XReader.Value == Boolean.FalseString) ? false : true;
                            }
                            else if (path == "\\Shutdown-Manager\\Machines\\Machine\\Category")
                            {
                                if (XReader.Value == MachineCategory.Critical.ToString())
                                {
                                    Machine.Category = MachineCategory.Critical;
                                }
                                else if (XReader.Value == MachineCategory.High.ToString())
                                {
                                    Machine.Category = MachineCategory.High;
                                }
                                else if (XReader.Value == MachineCategory.NonCritical.ToString())
                                {
                                    Machine.Category = MachineCategory.NonCritical;
                                }
                                else
                                {
                                    if (!ShowedHelp)
                                    {
                                        MessageBox.Show("A machine has a duplicate name or non existant category field. It has been removed or put into NonCritical category.");
                                    }
                                    ShowedHelp = true;
                                    Machine.Category = MachineCategory.NonCritical;
                                }
                            }
                            sectiontree.RemoveAt(sectiontree.Count - 1);
                            Console.WriteLine(XReader.ReadContentAsString());
                        }
                        else if (XReader.NodeType == XmlNodeType.EndElement)
                        {
                            sectiontree.RemoveAt(sectiontree.Count - 1);
                            if (XReader.Name == "Machine")
                            {
                                if (!MachineFail)
                                {
                                    Machines.Add(Machine);
                                }
                                MachineFail = false;
                                Machine = new MachineInfo();
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                return e.ToString();
            }
            return "";
        }

        static string ToPath(List<string> ToConv)
        {
            string Total = "";
            foreach (string st in ToConv)
            {
                Total += "\\" + st;
            }
            return Total;
        }

        internal static string Save()
        {
            try
            {
                using (StreamWriter SW = new StreamWriter(new FileStream(Directory + "\\settings.xml", FileMode.Create)))
                {
                    //Write head
                    SW.WriteLine("<Shutdown-Manager>");

                    #region Settings
                    SW.WriteLine("\t<Settings>");//Head

                    SW.WriteLine("\t\t<ShowConfirmation>" + ShowConfirmation.ToString() + "</ShowConfirmation>");
                    SW.WriteLine("\t\t<CommandTimeout>" + (CommandTimeout >= MinCommandTimeout ? CommandTimeout.ToString() : MinCommandTimeout.ToString()) + "</CommandTimeout>");
                    //Ping Settings
                    SW.WriteLine("\t\t<PingInterval>"+(PingInterval>=MinPingInterval? PingInterval.ToString():MinPingInterval.ToString())+"</PingInterval>");
                    SW.WriteLine("\t\t<PingTimeout>"+(PingTimeout>=MinPingTimeout?PingTimeout.ToString():MinPingTimeout.ToString())+"</PingTimeout>");
                    //Restart Settings
                    SW.WriteLine("\t\t<ForceRestart>"+ForceRestart.ToString()+"</ForceRestart>");
                    SW.WriteLine("\t\t<DelayRestart>"+(DelayRestart>=MinDelayRestart?DelayRestart.ToString():MinDelayRestart.ToString())+"</DelayRestart>");
                    SW.WriteLine("\t\t<RestartMessage Show=\""+ShowRestartMessage.ToString()+"\">"+RestartMessage+"</RestartMessage>");
                    //Shutdown Settings
                    SW.WriteLine("\t\t<ForceShutdown>" + ForceShutdown.ToString() + "</ForceShutdown>");
                    SW.WriteLine("\t\t<DelayShutdown>" + (DelayShutdown>=MinDelayShutdown?DelayShutdown.ToString():MinDelayShutdown.ToString() )+ "</DelayShutdown>");
                    SW.WriteLine("\t\t<ShutdownMessage Show=\"" + ShowShutdownMessage.ToString() + "\">" + ShutdownMessage + "</ShutdownMessage>");

                    SW.WriteLine("\t</Settings>");//End
                    #endregion

                    #region Machines
                    SW.WriteLine("\t<Machines>");//Head

                    foreach (MachineInfo MI in machines)
                    {
                        SW.WriteLine("\t\t<Machine>");//Head

                        SW.WriteLine("\t\t\t<Name>"+MI.FQDN+"</Name>");
                        SW.WriteLine("\t\t\t<FriendlyName>" + MI.FriendlyName + "</FriendlyName>");
                        SW.WriteLine("\t\t\t<Dependency>" + MI.DependentOn + "</Dependency>");
                        SW.WriteLine("\t\t\t<Active>" + MI.Active.ToString() + "</Active>");
                        SW.WriteLine("\t\t\t<Category>" + MI.Category.ToString() + "</Category>");

                        SW.WriteLine("\t\t</Machine>");//Head
                    }

                    SW.WriteLine("\t</Machines>");//End
                    #endregion

                    //Write end
                    SW.WriteLine("</Shutdown-Manager>");
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }
    }
}
