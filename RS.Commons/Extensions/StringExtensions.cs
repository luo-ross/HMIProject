using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RS.Commons.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 判断是否为ip 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsIP(this string value)
        {
            return Regex.IsMatch(value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        public static bool IsIPPort(string port)
        {
            int portNum;
            return Int32.TryParse(port, out portNum) && portNum >= 0 && portNum <= 65535 ? true : false;
        }


        public static bool IsMatch(this string value, string pattern)
        {
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsMatch(this string value, string pattern, RegexOptions regexOptions)
        {
            return Regex.IsMatch(value, pattern, regexOptions);
        }

    

        /// <summary>
        /// 判断字符串是否为空或空白字符
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <returns>是否为空或空白字符</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 将字符串转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的值</returns>
        public static T To<T>(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            try
            {
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 移除字符串中的HTML标签
        /// </summary>
        /// <param name="str">包含HTML标签的字符串</param>
        /// <returns>移除HTML标签后的字符串</returns>
        public static string StripHtml(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return Regex.Replace(str, "<.*?>", string.Empty);
        }

        /// <summary>
        /// 截取指定长度的字符串，超出部分用省略号表示
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="length">要截取的长度</param>
        /// <returns>截取后的字符串</returns>
        public static string Truncate(this string str, int length)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= length ? str : str.Substring(0, length) + "...";
        }
    }
}
