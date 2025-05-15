using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class LoadDataArgs
    {
        /// <summary>
        /// 每页行数  
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 当前页  
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 排序列  
        /// </summary>
        public string Sidx { get; set; } = "Id";

        /// <summary>
        /// 排序类型  
        /// </summary>
        public string Sord { get; set; } = "desc";

        /// <summary>
        /// 取消
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
