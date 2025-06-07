using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    public class ProptertyConfig
    {

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 属性描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public bool IsCanEidt { get; set; }

        /// <summary>
        /// 是否支持查询
        /// </summary>
        public bool IsCanSearch { get; set; }
    }
}
