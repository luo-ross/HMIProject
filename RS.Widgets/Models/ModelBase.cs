using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class ModelBase : NotifyBase
    {
        /// <summary>
        /// 是否正在加载中
        /// </summary>
        public bool IsLoading { get; set; }
    }
}
