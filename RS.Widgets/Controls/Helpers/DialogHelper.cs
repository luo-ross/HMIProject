using Microsoft.IdentityModel.Tokens;
using RS.Widgets.Interfaces;
using RS.Win32API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Controls
{
    public static class DialogHelper
    {

        private static readonly ConcurrentDictionary<object, IDialog> DialogDic = new();

        public static void RegisterDialog(object dataContext, IDialog dialog)
        {
            if (dataContext != null && !DialogDic.ContainsKey(dataContext))
            {
                DialogDic[dataContext] = dialog;
            }
        }

        public static void UnregisterDialog(object dataContext)
        {
            if (dataContext != null && DialogDic.ContainsKey(dataContext))
            {
                DialogDic.TryRemove(dataContext, out IDialog dialog);
            }
        }

        public static IDialog? GetDialog(object dataContext)
        {
            if (dataContext == null)
            {
                return default;
            }
            DialogDic.TryGetValue(dataContext, out var dialog);
            return dialog;
        }

    }
}
