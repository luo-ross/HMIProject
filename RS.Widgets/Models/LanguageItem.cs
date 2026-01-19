using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class LanguageItem
    {
        /// <summary>
        /// 语言显示名称（如"简体中文"、"English (United States)"）
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// IETF语言标签（如zh-CN、en-US）
        /// </summary>
        public string? LanguageTag { get; set; }

        /// <summary>
        /// 文化信息对象
        /// </summary>
        public CultureInfo? Culture { get; set; }
    }
}
