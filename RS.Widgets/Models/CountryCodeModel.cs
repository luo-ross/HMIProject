using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 国家代码类
    /// </summary>
    public class CountryCodeModel
    {
        /// <summary>
        /// 国家名称
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        public int CountryCode { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string? GroupName { get; set; }
    }
}
