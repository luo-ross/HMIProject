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
    /// 日历项日模型
    /// </summary>
    public class CalendarDayModel : CalendarBaseModel
    {

        private bool isFirstDayOfMonth;
        /// <summary>
        /// 是否是月份的第一天
        /// </summary>
        public bool IsFirstDayOfMonth
        {
            get { return isFirstDayOfMonth; }
            set { SetProperty(ref isFirstDayOfMonth, value); }
        }

        private bool isToday;
        /// <summary>
        /// 是否是今天
        /// </summary>
        public bool IsToday
        {
            get { return isToday; }
            set { SetProperty(ref isToday, value); }
        }

        private bool isCurrentMonth;
        /// <summary>
        /// 是否是当前月份
        /// </summary>
        public bool IsCurrentMonth
        {
            get { return isCurrentMonth; }
            set { SetProperty(ref isCurrentMonth, value); }
        }

        private string monthName;
        /// <summary>
        /// 月份名称
        /// </summary>
        public string MonthName
        {
            get { return monthName; }
            set { SetProperty(ref monthName, value); }
        }

    }
}
