using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    public class EmailRegisterVerificationModel
    {
        public string Email { get; set; }
        public string Verification { get; set; }
    }
}
