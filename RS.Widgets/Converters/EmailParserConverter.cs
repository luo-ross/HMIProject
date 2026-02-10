using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    /// <summary>
    /// 邮箱地址解析转换器
    /// Parameter = "Name" → 返回@前的用户名 | Parameter = "Domain" → 返回@后的域名
    /// </summary>
    public class EmailParserConverter : IValueConverter
    {
        /// <summary>
        /// 正向转换：邮箱地址 → 用户名/域名
        /// </summary>
        /// <param name="value">传入的邮箱地址（string类型）</param>
        /// <param name="targetType">目标类型（未使用）</param>
        /// <param name="parameter">解析类型（"Name"或"Domain"）</param>
        /// <param name="culture">区域信息（未使用）</param>
        /// <returns>解析后的用户名/域名，解析失败返回空字符串</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. 空值校验：传入的值为空直接返回空
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return string.Empty;
            }

            // 2. 转换为字符串并去除首尾空格
            string email = value.ToString().Trim();

            // 3. 校验邮箱格式（至少包含一个@）
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
            {
                return string.Empty; // 格式错误返回空
            }

            // 4. 获取参数并统一转为大写（避免大小写敏感问题）
            string parseType = parameter?.ToString()?.Trim().ToUpper() ?? string.Empty;

            // 5. 根据参数解析对应部分
            return parseType switch
            {
                "NAME" => email.Substring(0, atIndex),       // Name → @前的用户名
                "DOMAIN" => email.Substring(atIndex + 1),    // Domain → @后的域名
                _ => string.Empty                            // 未知参数返回空
            };
        }

        /// <summary>
        /// 反向转换：本场景不需要，直接抛出异常
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("不支持反向转换（用户名/域名→邮箱地址）");
        }
    }
}
