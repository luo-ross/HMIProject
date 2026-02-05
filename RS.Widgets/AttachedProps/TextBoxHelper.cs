using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    public static class TextBoxHelper
    {

        #region 水印设置

        public static readonly DependencyProperty WatermarkProperty =
  DependencyProperty.RegisterAttached("Watermark", typeof(object), typeof(TextBoxHelper), new PropertyMetadata("请输入内容"));

        public static object GetWatermark(DependencyObject obj)
        {
            return (object)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, object value)
        {
            obj.SetValue(WatermarkProperty, value);
        }


        public static readonly DependencyProperty RightContentProperty =
  DependencyProperty.RegisterAttached("RightContent", typeof(object), typeof(TextBoxHelper), new PropertyMetadata(null));

        public static object GetRightContent(DependencyObject obj)
        {
            return (object)obj.GetValue(RightContentProperty);
        }

        public static void SetRightContent(DependencyObject obj, object value)
        {
            obj.SetValue(RightContentProperty, value);
        }



        public static readonly DependencyProperty WatermarkForgroundProperty =
            DependencyProperty.RegisterAttached("WatermarkForground", typeof(Brush), typeof(TextBoxHelper), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#949494"))));

        public static Brush GetWatermarkForground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(WatermarkForgroundProperty);
        }

        public static void SetWatermarkForground(DependencyObject obj, Brush value)
        {
            obj.SetValue(WatermarkForgroundProperty, value);
        }

        #endregion

        #region 判断是否有文本
        public static readonly DependencyProperty HasTextProperty =
DependencyProperty.RegisterAttached("HasText", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetHasText(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasTextProperty);
        }
        public static void SetHasText(DependencyObject obj, bool value)
        {
            obj.SetValue(HasTextProperty, value);
        }
        #endregion

        #region 获取文本长度
        public static readonly DependencyProperty TextLengthProperty =
DependencyProperty.RegisterAttached("TextLength", typeof(int), typeof(TextBoxHelper), new PropertyMetadata(0));

        public static int GetTextLength(DependencyObject element)
        {
            return (int)element.GetValue(TextLengthProperty);
        }
        public static void SetTextLength(DependencyObject element, int value)
        {
            element.SetValue(TextLengthProperty, value);
        }
        #endregion

        #region 是否在聚焦的时候全选内容
        public static readonly DependencyProperty SelectAllOnFocusProperty =
DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false));

        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }
        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }
        #endregion

        #region 添加一个监视器
        public static readonly DependencyProperty IsMonitoringProperty =
DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(TextBoxHelper), new UIPropertyMetadata(false, OnIsMonitoringChanged));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.Dispatcher.BeginInvoke(() => TextBox_TextChanged(textBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None)), DispatcherPriority.Background);
                    textBox.TextChanged += TextBox_TextChanged;
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
                }
                else
                {
                    textBox.TextChanged -= TextBox_TextChanged;
                    textBox.GotFocus -= TextBox_GotFocus;
                    textBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
                }
            }
            else if (d is RichTextBox richTextBox)
            {
                if ((bool)e.NewValue)
                {
                    richTextBox.Dispatcher.BeginInvoke(() => RichTextBox_TextChanged(richTextBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None)), DispatcherPriority.Background);
                    richTextBox.TextChanged += RichTextBox_TextChanged;
                    richTextBox.GotFocus += RichTextBox_GotFocus;
                    richTextBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
                }
                else
                {
                    richTextBox.TextChanged -= RichTextBox_TextChanged;
                    richTextBox.GotFocus -= RichTextBox_GotFocus;
                    richTextBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
                }
            }
            else if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.Dispatcher.BeginInvoke(() => PasswordBox_PasswordChanged(passwordBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None)), DispatcherPriority.Background);
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                    passwordBox.GotFocus += PasswordBox_GotFocus;
                    passwordBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                    passwordBox.GotFocus -= PasswordBox_GotFocus;
                    passwordBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
                }
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as TextBoxBase, textBox => textBox?.SelectAll());
        }

        private static void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as PasswordBox, passwordBox => passwordBox?.SelectAll());
        }

        private static void RichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as RichTextBox, richTextBox => richTextBox?.SelectAll());
        }


        private static void ControlGotFocus<TDependencyObject>(TDependencyObject? sender, Action<TDependencyObject> action) where TDependencyObject : DependencyObject
        {
            if (sender != null)
            {
                if (GetSelectAllOnFocus(sender))
                {
                    sender.Dispatcher.BeginInvoke(action, sender);
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetTextLength(sender as PasswordBox, passwordBox =>
            {
                return passwordBox.Password.Length;
            });
        }

        private static void UIElementPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is UIElement element && !element.IsKeyboardFocusWithin && GetSelectAllOnFocus(element))
            {
                element.Focus();
                e.Handled = true;
            }
        }



        private static void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            SetTextLength(sender as TextBox, textBox =>
            {
                return textBox.Text.Length;
            });
        }

        private static void SetTextLength<TDependencyObject>(TDependencyObject? sender, Func<TDependencyObject, int> funcTextLength) where TDependencyObject : DependencyObject
        {
            if (sender != null)
            {
                var value = funcTextLength(sender);
                sender.SetValue(TextLengthProperty, value);
                sender.SetCurrentValue(HasTextProperty, value > 0 ? true : false);
            }
        }

        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }
        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }
        #endregion

        #region 是否显示清除按钮
        public static readonly DependencyProperty IsShowClearButtonProperty =
DependencyProperty.RegisterAttached("IsShowClearButton", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, OnIsShowClearButtonPropertyChanged));

        private static void OnIsShowClearButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_TextChanged;
                textBox.Loaded += TextBox_TextChanged;
                if (textBox.IsLoaded)
                {
                    TextBox_TextChanged(textBox, new RoutedEventArgs());
                }
            }
            else if (d is RichTextBox richTextBox)
            {
                richTextBox.Loaded -= RichTextBox_TextChanged;
                richTextBox.Loaded += RichTextBox_TextChanged;
                if (richTextBox.IsLoaded)
                {
                    TextBox_TextChanged(richTextBox, new RoutedEventArgs());
                }
            }
            else if (d is PasswordBox passwordBox)
            {
                passwordBox.Loaded -= PasswordBox_PasswordChanged;
                passwordBox.Loaded += PasswordBox_PasswordChanged;
                if (passwordBox.IsLoaded)
                {
                    PasswordBox_PasswordChanged(passwordBox, new RoutedEventArgs());
                }
            }
        }

        private static void RichTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            SetTextLength(sender as RichTextBox, richTextBox =>
            {
                var text = GetRichTextBoxText(richTextBox);
                return text.Length;
            });
        }

        private static string GetRichTextBoxText(RichTextBox richTextBox)
        {
            // 从文档开始到结束创建文本范围
            TextRange textRange = new TextRange(
                richTextBox.Document.ContentStart,  // 起始位置
                richTextBox.Document.ContentEnd      // 结束位置
            );

            // 返回纯文本
            return textRange.Text.Trim();
        }

        public static bool GetIsShowClearButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsShowClearButtonProperty);
        }
        public static void SetIsShowClearButton(DependencyObject obj, bool value)
        {
            obj.SetValue(IsShowClearButtonProperty, value);
        }
        #endregion

    }
}
