using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using RS.Widgets.Controls;

namespace RS.Widgets.Behaviors
{
    public class PasswordBoxBindBehavior : Behavior<PasswordBox>
    {

        #region 密码设置
        public static readonly DependencyProperty PasswordProperty =
DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxBindBehavior), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                if (!GetIsChanging(passwordBox))
                {
                    passwordBox.Password = (string)e.NewValue;
                }
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetIsChanging(passwordBox, true);
                SetPassword(passwordBox, passwordBox.Password);
                SetIsChanging(passwordBox, false);
            }
        }

        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }
        #endregion

        #region 密码是否发生改变
        public static readonly DependencyProperty IsChangingProperty =
DependencyProperty.RegisterAttached("IsChanging", typeof(bool), typeof(PasswordBoxBindBehavior), new FrameworkPropertyMetadata(false));

        public static bool GetIsChanging(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsChangingProperty);
        }
        public static void SetIsChanging(DependencyObject obj, bool value)
        {
            obj.SetValue(IsChangingProperty, value);
        }
        #endregion

        #region 保存密码显示框
        public static readonly DependencyProperty PasswordTextBoxProperty =
DependencyProperty.RegisterAttached("PasswordTextBox", typeof(TextBox), typeof(PasswordBoxBindBehavior), new FrameworkPropertyMetadata(default(TextBox)));

        public static TextBox GetPasswordTextBox(DependencyObject obj)
        {
            return (TextBox)obj.GetValue(PasswordTextBoxProperty);
        }
        public static void SetPasswordTextBox(DependencyObject obj, TextBox? value)
        {
            obj.SetValue(PasswordTextBoxProperty, value);
        }
        #endregion

        #region 处理密码选择光标
        public static readonly DependencyProperty SelectionProperty =
DependencyProperty.RegisterAttached("Selection", typeof(TextSelection), typeof(PasswordBoxBindBehavior), new FrameworkPropertyMetadata(default(TextSelection)));

        public static TextSelection GetSelection(DependencyObject obj)
        {
            return (TextSelection)obj.GetValue(SelectionProperty);
        }
        public static void SetSelection(DependencyObject obj, TextSelection? value)
        {
            obj.SetValue(SelectionProperty, value);
        }
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.PasswordChanged += PasswordBox_PasswordChanged;
            this.AssociatedObject.Loaded += PasswordBox_Loaded;
            var selection = GetSelection(this.AssociatedObject);
            if (selection != null)
            {
                selection.Changed += Selection_Changed;
            }
        }

        private void Selection_Changed(object? sender, EventArgs e)
        {
            SetPasswordTextBoxCaretIndex(this.AssociatedObject);
        }

        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            SetPassword(this.AssociatedObject, this.AssociatedObject.Password);
            var textBox = this.AssociatedObject.FindChild<TextBox>("PART_PasswordTextBox");
            if (textBox != null)
            {
                var selection = GetSelection(this.AssociatedObject);
                if (selection != null)
                {
                    var infos = this.AssociatedObject.GetType().GetProperty("Selection", BindingFlags.NonPublic | BindingFlags.Instance);
                    selection = infos?.GetValue(this.AssociatedObject, null) as TextSelection;
                    SetSelection(this.AssociatedObject, selection);
                    if (selection != null)
                    {
                        SetPasswordTextBox(this.AssociatedObject, textBox);
                        SetPasswordTextBoxCaretIndex(this.AssociatedObject);
                    }

                }
            }
        }

        private void SetPasswordTextBoxCaretIndex(PasswordBox passwordBox)
        {
            var textbox = GetPasswordTextBox(passwordBox);
            if (textbox != null)
            {
                var caretPos = GetPasswordBoxCaretPosition(passwordBox);
                textbox.CaretIndex = caretPos;
            }
        }

        private int GetPasswordBoxCaretPosition(PasswordBox passwordBox)
        {
            var selection = GetSelection(passwordBox);
            var textRange = selection?.GetType().GetInterfaces().FirstOrDefault(t => t.Name == "ITextRange");
            var start = textRange?.GetProperty("Start")?.GetGetMethod()?.Invoke(selection, null);
            var value = start?.GetType().GetProperty("Offset", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(start, null) as int?;
            var caretPostion = value.GetValueOrDefault(0);
            return caretPostion;
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                var selection=GetSelection(this.AssociatedObject);
                if (selection!=null)
                {
                    selection .Changed-= this.Selection_Changed;
                }

                this.AssociatedObject.PasswordChanged -= PasswordBox_PasswordChanged;
                this.AssociatedObject.Loaded -= PasswordBox_Loaded;
            }
        }

    }
}
