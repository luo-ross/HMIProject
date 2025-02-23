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
    }
}
