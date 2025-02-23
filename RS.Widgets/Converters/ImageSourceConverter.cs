using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Interop;

namespace RS.Widgets.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            var path = value.ToString();
            if (string.IsNullOrEmpty(path))
            {
                return DependencyProperty.UnsetValue;
            }

            if (!File.Exists(path))
            {
                return DependencyProperty.UnsetValue;
            }

            int.TryParse(parameter as string, out int decodePixelWidth);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.DecodePixelWidth = decodePixelWidth;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
