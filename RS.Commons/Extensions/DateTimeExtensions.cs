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
       
    }
}
