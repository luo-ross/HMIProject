﻿using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using RS.Commons.Extend;
using RS.Commons.Extensions;

namespace RS.Commons.Helper
{
    public static class ValidHelper
    {

        #region IsEmail(是否邮箱)
        /// <summary>
        /// 是否邮箱
        /// </summary>
        /// <param name="value">邮箱地址</param>
        /// <returns></returns>
        public static bool IsEmail(this string value, bool isRestrict = false)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string pattern = isRestrict
                ? @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$"
                : @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";

            return value.IsMatch(pattern, RegexOptions.IgnoreCase);
        }




        /// <summary>
        /// 是否存在邮箱
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="isRestrict">是否按严格模式验证</param>
        /// <returns></returns>
        public static bool HasEmail(this string value, bool isRestrict = false)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string pattern = isRestrict
                ? @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$"
                : @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            return value.IsMatch(pattern, RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsPhoneNumber(是否合法的手机号码)
        /// <summary>
        /// 是否合法的手机号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <returns></returns>
        public static bool IsPhoneNumber(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^(0|86|17951)?(13[0-9]|15[012356789]|18[0-9]|14[57]|17[678])[0-9]{8}$");
        }
        #endregion

        #region IsMobileNumber(是否手机号码)
        /// <summary>
        /// 是否手机号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <param name="isRestrict">是否按严格模式验证</param>
        /// <returns></returns>
        public static bool IsMobileNumberSimple(this string value, bool isRestrict = false)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string pattern = isRestrict ? @"^[1][3-8]\d{9}$" : @"^[1]\d{10}$";
            return value.IsMatch(pattern);
        }
        /// <summary>
        /// 是否手机号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <returns></returns>
        public static bool IsMobileNumber(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            value = value.Trim().Replace("^", "").Replace("$", "");
            /**
             * 手机号码: 
             * 13[0-9], 14[5,7], 15[0, 1, 2, 3, 5, 6, 7, 8, 9], 17[6, 7, 8], 18[0-9], 170[0-9]
             * 移动号段: 134,135,136,137,138,139,150,151,152,157,158,159,182,183,184,187,188,147,178,1705
             * 联通号段: 130,131,132,155,156,185,186,145,176,1709
             * 电信号段: 133,153,180,181,189,177,1700
             */
            return value.IsMatch(@"^1(3[0-9]|4[57]|5[0-35-9]|8[0-9]|70)\d{8}$");
        }

        /// <summary>
        /// 是否存在手机号码
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="isRestrict">是否按严格模式验证</param>
        /// <returns></returns>
        public static bool HasMobileNumberSimple(this string value, bool isRestrict = false)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string pattern = isRestrict ? @"[1][3-8]\d{9}" : @"[1]\d{10}";
            return value.IsMatch(pattern);
        }
        #endregion

        #region IsChinaMobilePhone(是否中国移动号码)
        /// <summary>
        /// 是否中国移动号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <returns></returns>
        public static bool IsChinaMobilePhone(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            /**
             * 中国移动：China Mobile
             * 134,135,136,137,138,139,150,151,152,157,158,159,182,183,184,187,188,147,178,1705
             */
            return value.IsMatch(@"(^1(3[4-9]|4[7]|5[0-27-9]|7[8]|8[2-478])\d{8}$)|(^1705\d{7}$)");
        }
        #endregion

        #region IsChinaUnicomPhone(是否中国联通号码)
        /// <summary>
        /// 是否中国联通号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <returns></returns>
        public static bool IsChinaUnicomPhone(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            /**
             * 中国联通：China Unicom
             * 130,131,132,155,156,185,186,145,176,1709
             */
            return value.IsMatch(@"(^1(3[0-2]|4[5]|5[56]|7[6]|8[56])\d{8}$)|(^1709\d{7}$)");
        }
        #endregion

        #region IsChinaTelecomPhone(是否中国电信号码)
        /// <summary>
        /// 是否中国电信号码
        /// </summary>
        /// <param name="value">手机号码</param>
        /// <returns></returns>
        public static bool IsChinaTelecomPhone(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            /**
             * 中国电信：China Telecom
             * 133,153,180,181,189,177,1700
             */
            return value.IsMatch(@"(^1(33|53|77|8[019])\d{8}$)|(^1700\d{7}$)");
        }
        #endregion

        #region IsIdCard(是否身份证号码)
        /// <summary>
        /// 是否身份证号码
        /// </summary>
        /// <param name="value">身份证</param>
        /// <returns></returns>
        public static bool IsIdCard(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            if (value.Length == 15)
            {
                return value.IsMatch(@"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$");
            }
            return value.Length == 0x12 &&
                   value.IsMatch(@"^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$",
                       RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsBase64String(是否Base64编码)
        /// <summary>
        /// 是否Base64编码
        /// </summary>
        /// <param name="value">Base64字符串</param>
        /// <returns></returns>
        public static bool IsBase64String(this string value)
        {
            return value.IsMatch(@"[A-Za-z0-9\+\/\=]");
        }
        #endregion

        #region IsDate(是否日期)
        /// <summary>
        /// 是否日期
        /// </summary>
        /// <param name="value">日期字符串</param>
        /// <param name="isRegex">是否正则验证</param>
        /// <returns></returns>
        public static bool IsDate(this string value, bool isRegex = false)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            if (isRegex)
            {
                //考虑到4年一度的366天，还有特殊的2月的日期
                return
                    value.IsMatch(
                        @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d$");
            }
            DateTime minValue;
            return DateTime.TryParse(value, out minValue);
        }

        /// <summary>
        /// 是否日期
        /// </summary>
        /// <param name="value">日期字符串</param>
        /// <param name="format">格式化字符串</param>
        /// <returns></returns>
        public static bool IsDate(this string value, string format)
        {
            return value.IsDate(format, null, DateTimeStyles.None);
        }

        /// <summary>
        /// 是否日期
        /// </summary>
        /// <param name="value">日期字符串</param>
        /// <param name="format">格式化字符串</param>
        /// <param name="provider">格式化提供者</param>
        /// <param name="styles">日期格式</param>
        /// <returns></returns>
        public static bool IsDate(this string value, string format, IFormatProvider provider, DateTimeStyles styles)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            DateTime minValue;
            return DateTime.TryParseExact(value, format, provider, styles, out minValue);
        }
        #endregion

        #region IsDateTime(是否有效时间)
        /// <summary>
        /// 是否大于最小时间
        /// </summary>
        /// <param name="value">时间</param>
        /// <param name="min">最小时间</param>
        /// <returns></returns>
        public static bool IsDateTimeMin(this string value, DateTime min)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            DateTime dateTime;
            if (DateTime.TryParse(value, out dateTime))
            {
                if (DateTime.Compare(dateTime, min) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 是否小于最大时间
        /// </summary>
        /// <param name="value">时间</param>
        /// <param name="max">最大时间</param>
        /// <returns></returns>
        public static bool IsDateTimeMax(this string value, DateTime max)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            DateTime dateTime;
            if (DateTime.TryParse(value, out dateTime))
            {
                if (DateTime.Compare(max, dateTime) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IsUrl(是否Url地址)
        /// <summary>
        /// 是否Url地址（统一资源定位）
        /// </summary>
        /// <param name="value">url地址</param>
        /// <returns></returns>
        public static bool IsUrl(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return
                value.IsMatch(
                    @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$",
                    RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsUri(是否Uri)
        /// <summary>
        /// 是否Uri（统一资源标识）
        /// </summary>
        /// <param name="value">uri</param>
        /// <returns></returns>
        public static bool IsUri(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            if (value.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }
            var schemes = new[]
            {
        "file",
        "ftp",
        "gopher",
        "http",
        "https",
        "ldap",
        "mailto",
        "net.pipe",
        "net.tcp",
        "news",
        "nntp",
        "telnet",
        "uuid"
    };

            bool hasValidSchema = false;
            foreach (string scheme in schemes)
            {
                if (hasValidSchema)
                {
                    continue;
                }
                if (value.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                {
                    hasValidSchema = true;
                }
            }
            if (!hasValidSchema)
            {
                value = "http://" + value;
            }
            return Uri.IsWellFormedUriString(value, UriKind.Absolute);
        }
        #endregion

        #region IsMainDomain(是否主域名)
        /// <summary>
        /// 是否主域名或者www开头的域名
        /// </summary>
        /// <param name="value">url地址</param>
        /// <returns></returns>
        public static bool IsMainDomain(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return
                value.IsMatch(
                    @"^http(s)?\://((www.)?[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        }
        #endregion

        #region IsGuid(是否Guid)
        /// <summary>
        /// 是否Guid
        /// </summary>
        /// <param name="guid">Guid字符串</param>
        /// <returns></returns>
        public static bool IsGuid(this string guid)
        {
            if (guid.IsEmpty())
            {
                return false;
            }
            return guid.IsMatch(@"[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}|[A-F0-9]{32}", RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsPositiveInteger(是否大于0的正整数)
        /// <summary>
        /// 是否大于0的正整数
        /// </summary>
        /// <param name="value">正整数</param>
        /// <returns></returns>
        public static bool IsPositiveInteger(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^[1-9]+\d*$");
        }
        #endregion

        #region IsInt32(是否Int32类型)
        /// <summary>
        /// 是否Int32类型
        /// </summary>
        /// <param name="value">整数</param>
        /// <returns></returns>
        public static bool IsInt32(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^[0-9]*$");
        }
        #endregion

        #region IsDouble(是否Double类型，如果带有.默认为1位0)
        /// <summary>
        /// 是否Double类型
        /// </summary>
        /// <param name="value">小数</param>
        /// <returns></returns>
        public static bool IsDouble(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^\d[.]?\d?$");
        }
        /// <summary>
        /// 是否Double类型
        /// </summary>
        /// <param name="value">小数</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="digit">小数位数，如果是0则不检测</param>
        /// <returns></returns>
        public static bool IsDouble(this string value, double minValue, double maxValue, int digit)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string patten = string.Format(@"^\d[.]?\d{0}$", "{0,10}");
            if (digit > 0)
            {
                patten = string.Format(@"^\d[.]?\d{0}$", "{" + digit + "}");
            }
            if (value.IsMatch(patten))
            {
                double val = Convert.ToDouble(value);
                if (val >= minValue && val <= maxValue)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IsInteger(是否整数)
        /// <summary>
        /// 是否整数
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>结果</returns>
        public static bool IsInteger(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^\-?[0-9]+$");
        }
        #endregion

        #region IsUnicode(是否Unicode字符串)
        /// <summary>
        /// 是否Unicode字符串
        /// </summary>
        /// <param name="value">unicode字符串</param>
        /// <returns>结果</returns>
        public static bool IsUnicode(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return
                value.IsMatch(
                    @"^(http|https|ftp|rtsp|mms):(\/\/|\\\\)[A-Za-z0-9%\-_@]+\.[A-Za-z0-9%\-_@]+[A-Za-z0-9\.\/=\?%\-&_~`@:\+!;]*$");
        }
        #endregion

        #region IsDecimal(是否数字型)
        /// <summary>
        /// 是否数字型
        /// </summary>
        /// <param name="value">数字</param>
        /// <returns></returns>
        public static bool IsDecimal(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^([0-9])[0-9]*(\.\w*)?$");
        }
        #endregion

        #region IsMac(是否Mac地址)
        /// <summary>
        /// 是否Mac地址
        /// </summary>
        /// <param name="value">Mac地址</param>
        /// <returns></returns>
        public static bool IsMac(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^([0-9A-F]{2}-){5}[0-9A-F]{2}$") || value.IsMatch(@"^[0-9A-F]{12}$");
        }
        #endregion

        #region IsIpAddress(是否IP地址)
        /// <summary>
        /// 是否IP地址
        /// </summary>
        /// <param name="value">ip地址</param>
        /// <returns>结果</returns>
        public static bool IsIpAddress(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^(\d(25[0-5]|2[0-4][0-9]|1?[0-9]?[0-9])\d\.){3}\d(25[0-5]|2[0-4][0-9]|1?[0-9]?[0-9])\d$");
        }
        #endregion

        #region IsVersion(是否有效的版本号)
        /// <summary>
        /// 是否有效版本号，范例：1.3,1.1.5,1.25.256
        /// </summary>
        /// <param name="value">版本号</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static bool IsVersion(this string value, int length = 5)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            value = value.Replace("^", "").Replace("$", "");
            return value.IsMatch(string.Format(@"^{0}{1}{2}$", @"\d{0,4}\.(\d{1,4}\.){0,", length, @"}\d{1,4}"));
        }
        #endregion

        #region IsContainsChinese(是否包含中文)
        /// <summary>
        /// 是否中文
        /// </summary>
        /// <param name="value">中文</param>
        /// <returns></returns>
        public static bool IsChinese(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^[\u4e00-\u9fa5]+$", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 是否包含中文
        /// </summary>
        /// <param name="value">中文</param>
        /// <returns></returns>
        public static bool IsContainsChinese(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"[\u4e00-\u9fa5]+", RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsContainsNumber(是否包含数字)
        /// <summary>
        /// 是否包含数字
        /// </summary>
        /// <param name="value">数字</param>
        /// <returns></returns>
        public static bool IsContainsNumber(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"[0-9]+");
        }
        #endregion

        #region IsLengthStr(字符串长度是否在指定范围内)
        /// <summary>
        /// 字符串长度是否在指定范围内，一个中文为2个字符
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="begin">开始</param>
        /// <param name="end">结束</param>
        /// <returns></returns>
        public static bool IsLengthStr(this string value, int begin, int end)
        {
            int length = Regex.Replace(value, @"[^\x00-\xff]", "OK").Length;
            if (length <= begin && length >= end)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region IsNormalChar(是否正常字符，字母、数字、下划线的组合)
        /// <summary>
        /// 是否正常字符，字母、数字、下划线的组合
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns></returns>
        public static bool IsNormalChar(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"[\w\d_]+", RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsPostfix(是否指定后缀)
        /// <summary>
        /// 是否指定后缀
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="postfixs">后缀名数组</param>
        /// <returns></returns>
        public static bool IsPostfix(this string value, string[] postfixs)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            string postfix = string.Join("|", postfixs);
            return value.IsMatch(string.Format(@".(?i:{0})$", postfix));
        }
        #endregion

        #region IsRepeat(是否重复)
        /// <summary>
        /// 是否重复，范例：112,返回true
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static bool IsRepeat(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            var array = value.ToCharArray();
            return array.Any(c => array.Count(t => t == c) > 1);
        }
        #endregion

        #region IsPostalCode(是否邮政编码)
        /// <summary>
        /// 是否邮政编码，6位数字
        /// </summary>
        /// <param name="value">邮政编码</param>
        /// <returns></returns>
        public static bool IsPostalCode(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^[1-9]\d{5}$", RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsTel(是否中国电话)
        /// <summary>
        /// 是否中国电话，格式：010-85849685
        /// </summary>
        /// <param name="value">电话</param>
        /// <returns></returns>
        public static bool IsTel(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^\d{3,4}-?\d{6,8}$", RegexOptions.IgnoreCase);
        }
        #endregion

        #region IsQQ(是否合法QQ号码)
        /// <summary>
        /// 是否合法QQ号码
        /// </summary>
        /// <param name="value">QQ号码</param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static bool IsQQ(this string value)
        {
            if (value.IsEmpty())
            {
                return false;
            }
            return value.IsMatch(@"^[1-9][0-9]{4,9}$");
        }
        #endregion

    }
}
