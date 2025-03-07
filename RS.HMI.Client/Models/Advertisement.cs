using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public class Advertisement
    {
        /// <summary>
        /// 广告宣传图连接
        /// </summary>
        public string PromotionalImageUrl { get; set; }

        /// <summary>
        /// 广告宣传跳转连接
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
