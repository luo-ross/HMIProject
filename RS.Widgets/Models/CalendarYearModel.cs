using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 日历项年模型
    /// </summary>
    public class CalendarYearModel : CalendarBaseModel
    {

        private bool isCurrentYear;
        /// <summary>
        /// 是否是当前年
        /// </summary>
        public bool IsCurrentYear
        {
            get { return isCurrentYear; }
            set { SetProperty(ref isCurrentYear, value); }
        }
    
    }
}
