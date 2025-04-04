using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    public class LoginClientModel
    {
        public string RemoteIpAddress { get; set; }
        public string LocalIpAddress { get; set; }
        public string XForwardedFor { get; set; }
        public string ClientIPHash { get; set; }
        public string UserAgent { get; set; }
    }
}
