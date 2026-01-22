using RS.WPFClient.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    public class MailFilterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mailFilterType = (MailFilterType)value;
            // 语言本地化待接入
            string description = "全部";
            switch (mailFilterType)
            {
                case MailFilterType.AllRead:
                    description = "全部";
                    break;
                case MailFilterType.Unread:
                    description = "未读";
                    break;
                case MailFilterType.WithAttachment:
                    description = "包含附件";
                    break;
                case MailFilterType.FromContact:
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

