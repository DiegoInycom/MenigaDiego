using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ibercaja.Utils.Config
{
    public class IbercajaConfiguration : ConfigurationSection, IIbercajaConfiguration
    {
        [ConfigurationProperty("notificationsHubUrl", IsRequired = false, DefaultValue = "http://localhost")]
        public string NotificationsHubUrl
        {
            get { return (string)this["notificationsHubUrl"]; }
            set { this["notificationsHubUrl"] = value; }
        }

        [ConfigurationProperty("jobsMaxProcessingThreads", IsRequired = false, DefaultValue = 4)]
        public int JobsMaxProcessingThreads
        {
            get { return (int)this["jobsMaxProcessingThreads"]; }
            set { this["jobsMaxProcessingThreads"] = value; }
        }
    }
}
