using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multi_Shutdown
{
    public class MachineInfo
    {
        public bool Active { get; set; }
        public string FriendlyName { get; set; }
        public string FQDN {get;set;}
        public MachineCategory Category { get; set; }
        private string dependentOn = "";
        private string[] dependentOnArray = null;
        public string DependentOn
        {
            get { return dependentOn; }
            set
            {
                dependentOn = value;
                if (!string.IsNullOrEmpty(DependentOn))
                {
                    dependentOnArray = DependentOn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    dependentOnArray = null;
                }
            }
        }

        public string[] GetDependentOnAsArray()
        {
            return dependentOnArray;
        }

        public MachineInfo Clone()
        {
            return (MachineInfo)this.MemberwiseClone();
        }
    }
    public enum MachineCategory { NonCritical, High, Critical }
}
