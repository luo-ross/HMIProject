using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 分页数据结果
    /// </summary>
    public class PageDataModel<T>
    {
        public Pagination Pagination { get; set; }

        public List<T> DataList { get; set; }
    }
}
