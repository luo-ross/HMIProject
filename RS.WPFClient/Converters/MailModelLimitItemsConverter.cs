using RS.WPFClient.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    public class EmailModelLimitItemsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is IEnumerable enumerable && values[1] is string account && !string.IsNullOrEmpty(account))
            {
                var mails = enumerable.Cast<object>().OfType<EmailModel>();

                // 根据 Account 筛选
                var filteredMails = mails.Where(t => t.Account == account && !t.IsHeader);

                // 按最新日期排序
                var sortedMails = filteredMails.OrderByDescending(m => m.Time);

                // 取前 limit 个
                if (parameter != null && int.TryParse(parameter.ToString(), out int limit))
                {
                    return sortedMails.Take(limit);
                }

                return sortedMails;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
