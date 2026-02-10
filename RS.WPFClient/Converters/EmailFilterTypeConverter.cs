using RS.WPFClient.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    public class EmailFilterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mailFilterType = (EmailFilterType)value;
            // 语言本地化待接入
            string description = "全部";
            switch (mailFilterType)
            {
                case EmailFilterType.AllRead:
                    description = "全部";
                    break;
                case EmailFilterType.Unread:
                    description = "未读";
                    break;
                case EmailFilterType.WithAttachment:
                    description = "包含附件";
                    break;
                case EmailFilterType.FromContact:
                    description = "来自联系人";
                    break;
            }
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}





