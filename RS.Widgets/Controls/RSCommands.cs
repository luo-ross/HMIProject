using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Controls;
using RS.Widgets.Behaviors;

namespace RS.Widgets.Controls
{
    public static class RSCommands
    {
        public static RoutedCommand CleanTextCommand { get; private set; }

        static RSCommands()
        {
            CleanTextCommand = new RoutedCommand("CleanText", typeof(RSCommands));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ClearControlCommand, ClearControl, CanClearControl));


        }

       
        public static void CleanText(object source)
        {
            if (source is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
            else if (source is PasswordBox passwordBox)
            {
                passwordBox.Password = string.Empty;
            }
            else if (source is ComboBox comboBox)
            {
                comboBox.SelectedValue = null;
                comboBox.SelectedItem = null;
            }
        }


        /// <summary>
        /// 清除控件内容
        /// </summary>
        public static RoutedCommand ClearControlCommand { get; } = new RoutedCommand("Clear", typeof(RSCommands));
        

        private static void CanClearControl(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject control || !TextBoxHelper.GetIsShowClearButton(control))
            {
                return;
            }

            e.CanExecute = control switch
            {
                TextBox textBox => !textBox.IsReadOnly,
                _ => true,
            };
        }

        private static void ClearControl(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject control || !TextBoxHelper.GetIsShowClearButton(control))
            {
                return;
            }

            switch (control)
            {
                case TextBox textBox:
                    textBox.Clear();
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    textBox.Focus();
                    break;
                case PasswordBox passwordBox:
                    passwordBox.Clear();
                    passwordBox.GetBindingExpression(PasswordBoxBindBehavior.PasswordProperty)?.UpdateSource();
                    passwordBox.Focus();
                    break;
            }
        }
    }
}
