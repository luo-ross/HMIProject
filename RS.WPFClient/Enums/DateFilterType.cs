using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{
    public enum DateFilterType
    {
        /// <summary>
        /// 任何时间（不限制时间范围）
        /// </summary>
        Any,

        /// <summary>
        /// 一天内（最近24小时内）
        /// </summary>
        InOneDay,

        /// <summary>
        /// 三天内（最近72小时内）
        /// </summary>
        InThreeDays,

        /// <summary>
        /// 一周内（最近7天内）
        /// </summary>
        InOneWeek,

        /// <summary>
        /// 两周内（最近14天内）
        /// </summary>
        InTwoWeeks,

        /// <summary>
        /// 一个月内（最近30天内，或当前月份内）
        /// </summary>
        WithinOneMonth,

        /// <summary>
        /// 两个月内（最近60天内，或最近两个自然月内）
        /// </summary>
        WithinTwoMonths,

        /// <summary>
        /// 半年内（最近6个月内）
        /// </summary>
        WithinSixMonths,

        /// <summary>
        /// 一年内（最近12个月内，或当前年内）
        /// </summary>
        InOneYear,

        /// <summary>
        /// 自定义时间范围（用户手动指定的起始和结束时间）
        /// </summary>
        Customise,
    }
}
