using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using RS.Widgets.Interface;
using System.ComponentModel;

namespace RS.Widgets.Controls
{
    public static class MessageBoxAttached
    {

        [Description("消息提示图标")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public static readonly DependencyProperty MessageBoxImageProperty =
            DependencyProperty.RegisterAttached("MessageBoxImage", typeof(MessageBoxImage), typeof(MessageBoxAttached), new PropertyMetadata(MessageBoxImage.None));
        public static MessageBoxImage GetMessageBoxImage(DependencyObject obj)
        {
            return (MessageBoxImage)obj.GetValue(MessageBoxImageProperty);
        }

        public static void SetMessageBoxImage(DependencyObject obj, MessageBoxImage value)
        {
            obj.SetValue(MessageBoxImageProperty, value);
        }

        [Description("消息框按钮")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public static readonly DependencyProperty MessageBoxButtonProperty =
            DependencyProperty.RegisterAttached("MessageBoxButton", typeof(MessageBoxButton), typeof(MessageBoxAttached), new PropertyMetadata(MessageBoxButton.OK));

        public static MessageBoxButton GetMessageBoxButton(DependencyObject obj)
        {
            return (MessageBoxButton)obj.GetValue(MessageBoxButtonProperty);
        }

        public static void SetMessageBoxButton(DependencyObject obj, MessageBoxButton value)
        {
            obj.SetValue(MessageBoxButtonProperty, value);
        }

      


        [Description("消息框内容")]
        [Category("消息框样式设置")]
        [Browsable(true)]

        public static readonly DependencyProperty MessageBoxTextProperty =
            DependencyProperty.RegisterAttached("MessageBoxText", typeof(string), typeof(MessageBoxAttached), new PropertyMetadata(null));

        public static string GetMessageBoxText(DependencyObject obj)
        {
            return (string)obj.GetValue(MessageBoxTextProperty);
        }

        public static void SetMessageBoxText(DependencyObject obj, string value)
        {
            obj.SetValue(MessageBoxTextProperty, value);
        }


        [Description("消息框标题")]
        [Category("消息框样式设置")]
        [Browsable(true)]

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.RegisterAttached("Caption", typeof(string), typeof(MessageBoxAttached), new PropertyMetadata("消息提示"));

        public static string GetCaption(DependencyObject obj)
        {
            return (string)obj.GetValue(CaptionProperty);
        }

        public static void SetCaption(DependencyObject obj, string value)
        {
            obj.SetValue(CaptionProperty, value);
        }


        [Description("消息框默认返回消息")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public static readonly DependencyProperty DefaultResultProperty =
            DependencyProperty.RegisterAttached("DefaultResult", typeof(MessageBoxResult), typeof(MessageBoxAttached), new PropertyMetadata(MessageBoxResult.None));

        public static MessageBoxResult GetDefaultResult(DependencyObject obj)
        {
            return (MessageBoxResult)obj.GetValue(DefaultResultProperty);
        }

        public static void SetDefaultResult(DependencyObject obj, MessageBoxResult value)
        {
            obj.SetValue(DefaultResultProperty, value);
        }


        [Description("指定消息框的特殊显示选项")]
        [Category("消息框样式设置")]
        [Browsable(true)]

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.RegisterAttached("Options", typeof(MessageBoxOptions), typeof(MessageBoxAttached), new PropertyMetadata(MessageBoxOptions.None));

        public static MessageBoxOptions GetOptions(DependencyObject obj)
        {
            return (MessageBoxOptions)obj.GetValue(OptionsProperty);
        }

        public static void SetOptions(DependencyObject obj, MessageBoxOptions value)
        {
            obj.SetValue(OptionsProperty, value);
        }




       
    }
}
