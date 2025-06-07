using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RS.Widgets.Models.Form
{
    public class PropertyBase : NotifyBase
    {



        private GridLength descriptionWidth = GridLength.Auto;
        /// <summary>
        /// 表单描述宽度
        /// </summary>
        public GridLength DescriptionWidth
        {
            get { return descriptionWidth; }
            set
            {
                this.SetProperty(ref descriptionWidth, value);
            }
        }



        private string description;
        /// <summary>
        /// 表单描述
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                this.SetProperty(ref description, value);
            }
        }

        /// <summary>
        /// 属性类型 用于判断是要用文本框 
        /// </summary>
        public PropertyType PropertyType { get; set; }


        private object? _value;
        /// <summary>
        /// 属性值
        /// </summary>
        public object? Value
        {
            get { return _value; }
            set
            {
                if (this.SetProperty(ref _value, value))
                {
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 属性值是否改变
        /// </summary>
        public bool IsChanged { get; set; }


        /// <summary>
        /// 是否运行编辑
        /// </summary>
        public bool IsCanEdit { get; set; }

        /// <summary>
        /// 验证回调
        /// </summary>
        public Func<bool> ValidCallBack { get; set; }

        /// <summary>
        /// 是否支持查询
        /// </summary>
        public bool IsCanSearch { get; set; }



        private bool isShowClearButton = true;
        /// <summary>
        /// 是否显示清除按钮
        /// </summary>
        public bool IsShowClearButton
        {
            get { return isShowClearButton; }
            set
            {
                this.SetProperty(ref isShowClearButton, value);
            }
        }


        private string watermark = "请输入内容";
        /// <summary>
        /// 自定义水印
        /// </summary>
        public virtual string Watermark
        {
            get { return watermark; }
            set
            {
                this.SetProperty(ref watermark, value);
            }
        }


        public static double GetMaxStringWidth(List<string> strings, double fontSize = 12)
        {
            var culture = CultureInfo.CurrentUICulture;
            string fontFamily = "Segoe UI";
            if (culture.Name.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase))
            {
                fontFamily = "Microsoft YaHei";
            }

            var typeface = new Typeface(fontFamily);


            double maxWidth = 0;
            foreach (var str in strings)
            {
                var ft = new FormattedText(
                    str,
                    culture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    Brushes.Black,
                    1.0
                );
                if (ft.Width > maxWidth)
                    maxWidth = ft.Width;
            }
            return maxWidth + 10;
        }
    }
}
