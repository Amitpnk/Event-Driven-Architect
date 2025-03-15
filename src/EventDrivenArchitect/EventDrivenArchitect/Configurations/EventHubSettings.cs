using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitect.Configurations
{
    public class EventHubSettings
    {
        public string ConnectionString { get; set; }
        public string EventHubName { get; set; }
        public string ConsumerGroup { get; set; }
    }

}
