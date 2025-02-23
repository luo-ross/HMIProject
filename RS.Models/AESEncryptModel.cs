using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// AES加密类
    /// </summary>
    public class AESEncryptModel : SignModel
    {
        /// <summary>
        /// 加密后的数据
        /// </summary>
        public string Encrypt { get; set; }
    }
}
