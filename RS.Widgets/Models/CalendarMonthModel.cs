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
    /// 日历项月模型
    /// </summary>
    public class CalendarMonthModel : CalendarBaseModel
    {
        private bool isCurrentMonth;
        /// <summary>
        /// 是否是当前月份
        /// </summary>
        public bool IsCurrentMonth
        {
            get { return isCurrentMonth; }
            set { SetProperty(ref isCurrentMonth, value); }
        }


        private bool isFirstMonthOfYear;
        /// <summary>
        /// 是否是当前年的第一个月
        /// </summary>
        public bool IsFirstMonthOfYear
        {
            get { return isFirstMonthOfYear; }
            set { SetProperty(ref isFirstMonthOfYear, value); }
        }

        private int year;
        /// <summary>
        /// 年
        /// </summary>
        public int Year
        {
            get { return year; }
            set { SetProperty(ref year, value); }
        }
    }
}
