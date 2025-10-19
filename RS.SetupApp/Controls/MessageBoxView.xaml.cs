using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RS.SetupApp.Controls
{
    /// <summary>
    /// MessageBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxView : RSWindow
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        public object MessageContent
        {
            get { return (object)GetValue(MessageContentProperty); }
            set { SetValue(MessageContentProperty, value); }
        }
        public static readonly DependencyProperty MessageContentProperty =
            DependencyProperty.Register("MessageContent", typeof(object), typeof(MessageBoxView), new PropertyMetadata(null));
    }
}
