using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 日期转长整型时间戳
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 日期转字符串时间戳
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns></returns>
        public static string ToTimeStampString(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds().ToString();
        }


        /// <summary>
        /// 将DateTime转换为Unix时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>Unix时间戳</returns>
        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            var timeSpan = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)timeSpan.TotalSeconds;
        }

        /// <summary>
        /// 将Unix时间戳转换为DateTime
        /// </summary>
        /// <param name="unixTimeStamp">Unix时间戳</param>
        /// <returns>DateTime对象</returns>
        public static DateTime FromUnixTimeStamp(this long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTimeStamp);
        }

        /// <summary>
        /// 获取当前时间的开始时间（00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的开始时间</returns>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 获取当前时间的结束时间（23:59:59）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天的结束时间</returns>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// 获取当前时间所在周的开始时间（周一）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当周的开始时间</returns>
        public static DateTime StartOfWeek(this DateTime dateTime)
        {
            int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// 获取当前时间所在月的开始时间
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当月的开始时间</returns>
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

    }
}
