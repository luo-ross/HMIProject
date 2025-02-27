using System;
using System.Text;

namespace RS.Commons.Extend
{
    public static partial class Ext
    {
        #region 数值转换
        /// <summary>
        /// 转换为整型 
        /// </summary>
        /// <param name="data">数据</param>
        public static int ToInt(this object data)
        {
            if (data == null)
                return 0;
            int result;
            var success = int.TryParse(data.ToString(), out result);
            if (success)
                return result;
            try
            {
                return Convert.ToInt32(ToDouble(data, 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }



        public static long ToLong(this object data)
        {
            if (data == null)
                return 0;
            long result;
            var success = long.TryParse(data.ToString(), out result);
            if (success)
                return result;
            try
            {
                return Convert.ToInt64(ToDouble(data, 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换为Byte
        /// </summary>
        /// <param name="data">数据</param>
        public static byte ToByte(this object data)
        {
            if (data == null)
                return 0;
            byte result;
            var success = byte.TryParse(data.ToString(), out result);
            if (success)
                return result;
            try
            {
                return Convert.ToByte(ToDouble(data, 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换为双精度浮点数 
        /// </summary>
        /// <param name="data">数据</param>
        public static double ToDouble(this object data)
        {
            if (data == null)
                return 0;
            double result;
            return double.TryParse(data.ToString(), out result) ? result : 0;
        }

        public static float ToFloat(this double data)
        {
            if (data > float.MaxValue)
                return float.MaxValue;
            if (data < float.MinValue)
                return float.MinValue;
            return (float)data;
        }

        /// <summary>
        /// 转换为双精度浮点数,并按指定的小数位4舍5入 
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static double ToDouble(this object data, int digits)
        {
            return Math.Round(ToDouble(data), digits);
        }


        /// <summary>
        /// 转换为高精度浮点数 
        /// </summary>
        /// <param name="data">数据</param>
        public static decimal ToDecimal(this object data)
        {
            if (data == null)
                return 0;
            decimal result;
            return decimal.TryParse(data.ToString(), out result) ? result : 0;
        }
        #endregion

        #region 日期转换
        /// <summary>
        /// 转换为日期 
        /// </summary>
        /// <param name="data">数据</param>
        public static DateTime ToDate(this object data)
        {
            if (data == null)
                return DateTime.MinValue;
            DateTime result;
            return DateTime.TryParse(data.ToString(), out result) ? result : DateTime.MinValue;
        }

        public static DateTime ToyyyyMMddDate(this string data)
        {
            return DateTime.ParseExact(data, "yyyyMMdd", null).ToDate();
        }

        public static long ToTimeStamp(this DateTime dtime)
        {
            TimeSpan tspan = dtime - new DateTime(1970, 1, 1);
            return Convert.ToInt64(tspan.TotalMilliseconds);
        }

        public static DateTime ToTimeDate(this long? timeStamp)
        {
            timeStamp = timeStamp ?? 0;
            return timeStamp.ToTimeDate();
        }

        public static DateTime ToTimeDate(this long timeStamp)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(timeStamp.ToDouble());
        }

        #endregion

        #region 字符串转换
        /// <summary>
        /// 是否为空 
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 是否为空 
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsEmpty(this object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 验证sql匹配条件是否正确(若以and开头则自动去除) 
        /// </summary>
        /// <param name="where">sql匹配条件</param>
        public static string CheckWhere(this string where)
        {
            string str = where.TrimStart();//去除前置空格
            if (str.ToLower().IndexOf("and ") == 0)//若以and开头则自动去除第一个and
            {
                where = str.Substring(4);//若要保留前面一个空格，可以改为3
            }
            else if (str.ToLower().IndexOf("or ") == 0)
            {
                where = str.Substring(3);//若要保留前面一个空格，可以改为2
            }
            return where;
        }
        #endregion

        #region 布尔转换
        /// <summary>
        /// 转换为布尔值 
        /// </summary>
        /// <param name="data">数据</param>
        public static bool ToBool(this object data)
        {
            if (data == null)
                return false;
            bool? value = GetBool(data);
            if (value != null)
                return value.Value;
            bool result;
            return bool.TryParse(data.ToString(), out result) && result;
        }

        /// <summary>
        /// 获取布尔值 
        /// </summary>
        private static bool? GetBool(this object data)
        {
            switch (data.ToString().Trim().ToLower())
            {
                case "0":
                    return false;
                case "1":
                    return true;
                case "是":
                    return true;
                case "否":
                    return false;
                case "yes":
                    return true;
                case "no":
                    return false;
                case "true":
                    return true;
                case "false":
                    return false;
                case "成功":
                    return true;
                case "失败":
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 转换为可空布尔值 
        /// </summary>
        /// <param name="data">数据</param>
        public static bool? ToBoolOrNull(this object data)
        {
            if (data == null)
                return null;
            bool? value = GetBool(data);
            if (value != null)
                return value.Value;
            bool result;
            bool isValid = bool.TryParse(data.ToString(), out result);
            if (isValid)
                return result;
            return null;
        }

        #endregion

        /// <summary>   
        /// 根据文件后缀来获取MIME类型字符串   
        /// </summary>   
        /// <param name="extension">文件后缀</param>   
        /// <returns></returns>   
        public static string ToMimeType(this string extension)
        {
            string mime = string.Empty;
            extension = extension.ToLower();
            switch (extension)
            {
                case ".avi": mime = "video/x-msvideo"; break;
                case ".bin":
                case ".exe":
                case ".msi":
                case ".dll":
                case ".class": mime = "application/octet-stream"; break;
                case ".csv": mime = "text/comma-separated-values"; break;
                case ".html":
                case ".htm":
                case ".shtml": mime = "text/html"; break;
                case ".css": mime = "text/css"; break;
                case ".js": mime = "text/javascript"; break;
                case ".doc":
                case ".dot":
                case ".docx": mime = "application/msword"; break;
                case ".xla":
                case ".xls":
                case ".xlsx": mime = "application/msexcel"; break;
                case ".ppt":
                case ".pptx": mime = "application/mspowerpoint"; break;
                case ".gz": mime = "application/gzip"; break;
                case ".gif": mime = "image/gif"; break;
                case ".bmp": mime = "image/bmp"; break;
                case ".jpeg":
                case ".jpg":
                case ".jpe":
                case ".png": mime = "image/jpeg"; break;
                case ".mpeg":
                case ".mpg":
                case ".mpe":
                case ".wmv": mime = "video/mpeg"; break;
                case ".mp3":
                case ".wma": mime = "audio/mpeg"; break;
                case ".pdf": mime = "application/pdf"; break;
                case ".rar": mime = "application/octet-stream"; break;
                case ".txt": mime = "text/plain"; break;
                case ".7z":
                case ".z": mime = "application/x-compress"; break;
                case ".zip": mime = "application/x-zip-compressed"; break;
                default:
                    mime = "application/octet-stream";
                    break;
            }
            return mime;
        }

        /// <summary>   
        /// 对字符串中的非 ASCII 字符进行编码   
        /// </summary>   
        /// <param name="s"></param>   
        /// <returns></returns>   
        public static string ToHexString(this string s)
        {
            char[] chars = s.ToCharArray();
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < chars.Length; index++)
            {
                bool needToEncode = NeedToEncode(chars[index]);
                if (needToEncode)
                {
                    string encodedString = ToHexString(chars[index]);
                    builder.Append(encodedString);
                }
                else
                {
                    builder.Append(chars[index]);
                }
            }
            return builder.ToString();
        }

        /// <summary>   
        /// 判断字符是否需要使用特殊的 ToHexString 的编码方式   
        /// </summary>   
        /// <param name="chr"></param>   
        /// <returns></returns>   
        private static bool NeedToEncode(char chr)
        {
            string reservedChars = "$-_.+!*'(),@=&";
            if (chr > 127)
                return true;
            if (char.IsLetterOrDigit(chr) || reservedChars.IndexOf(chr) >= 0)
                return false;
            return true;
        }

        /// <summary>   
        /// 为非 ASCII 字符编码   
        /// </summary>   
        /// <param name="chr"></param>   
        /// <returns></returns>   
        private static string ToHexString(char chr)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] encodedBytes = utf8.GetBytes(chr.ToString());
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < encodedBytes.Length; index++)
            {
                builder.AppendFormat("%{0}", Convert.ToString(encodedBytes[index], 16));
            }
            return builder.ToString();
        }



    }
}

