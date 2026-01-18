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
    /// 日历项模型
    /// </summary>
    public class CalendarBaseModel : NotifyBase
    {

        private CalendarViewType calendarViewType;
        /// <summary>
        /// 日历类型
        /// </summary>
        public CalendarViewType CalendarViewType
        {
            get { return calendarViewType; }
            set { SetProperty(ref calendarViewType, value); }
        }

        private string displayContent = string.Empty;
        /// <summary>
        /// 显示内容
        /// </summary>
        public string DisplayContent
        {
            get { return displayContent; }
            set { SetProperty(ref displayContent, value); }
        }

    
        private DateTime date;
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        private bool isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }


        private bool isBlackout;
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsBlackout
        {
            get { return isBlackout; }
            set { SetProperty(ref isBlackout, value); }
        }


        private ICommand calendarItemClickCommand;
        /// <summary>
        /// 点击命令
        /// </summary>
        public ICommand CalendarItemClickCommand
        {
            get { return calendarItemClickCommand; }
            set { SetProperty(ref calendarItemClickCommand, value); }
        }
    }
}
