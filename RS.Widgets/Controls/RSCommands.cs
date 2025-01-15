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
using RS.Widgets.Common.Commands;

namespace RS.Widgets.Controls
{
    public static class RSCommands
    {
     
        public static RoutedCommand CleanTextCommand { get; private set; }

        static RSCommands()
        {
            CleanTextCommand = new RoutedCommand("CleanText", typeof(RSCommands));
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
        }
    }
}
