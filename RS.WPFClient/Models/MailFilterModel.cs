using RS.WPFClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Models
{
    public class MailFilterModel
    {
       
        /// <summary>
        /// 邮件筛选类型
        /// </summary>
        public MailFilterType MailFilterType { get; set; }

        /// <summary>
        /// 排序顺序枚举
        /// </summary>
        public MailSortType MailSortType { get; set; }
    }
}
