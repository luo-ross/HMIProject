using RS.Commons.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Messages
{
    public class FormMessageBase
    {
        public CRUD CRUD { get; set; }

        public ModelBase ViewModel { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public object FormData { get; set; }
    }
}
