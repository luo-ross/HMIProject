using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    /// <summary>
    /// 安全类
    /// </summary>
    public class EmailSecurityModel : EmailModel
    {
        /// <summary>
        /// 重置链接
        /// </summary>
        public string? ResetLink { get; set; }

    }
}
