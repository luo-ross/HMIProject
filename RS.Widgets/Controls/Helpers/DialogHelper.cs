using Microsoft.IdentityModel.Tokens;
using RS.Widgets.Interfaces;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Controls
{
    public static class DialogHelper
    {

        private static readonly Dictionary<object, IDialog> DialogList = new();

        public static void RegisterDialog(object dataContext, IDialog dialog)
        {
            if (dataContext != null)
            {
                DialogList[dataContext] = dialog;
            }
        }

        public static void UnregisterDialog(object dataContext)
        {
            if (dataContext != null && DialogList.ContainsKey(dataContext))
            {
                DialogList.Remove(dataContext);
            }
        }

        public static IDialog? GetDialog(object dataContext)
        {
            if (dataContext == null)
            {
                return default;
            }
            DialogList.TryGetValue(dataContext, out var dialog);
            return dialog;
        }

    }
}
