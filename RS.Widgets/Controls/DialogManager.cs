using RS.Widgets.Interface;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Controls
{
    public static class DialogManager
    {

        private static readonly Dictionary<string, IDialog> DialogList = new();

        public static void RegisterDialog(string dialogKey, IDialog dialog)
        {
            if (!string.IsNullOrEmpty(dialogKey))
            {
                DialogList[dialogKey] = dialog;
            }
        }

        public static void UnregisterDialog(string dialogKey)
        {
            if (!string.IsNullOrEmpty(dialogKey)&& DialogList.ContainsKey(dialogKey))
            {
                DialogList.Remove(dialogKey);
            }
                
        }

        public static IDialog? GetDialog(string dialogKey, bool isUseWindow = false)
        {
            if (string.IsNullOrEmpty(dialogKey))
            {
                return default;
            }
            DialogList.TryGetValue(dialogKey, out var dialog);
            if (isUseWindow
                && dialog != null)
            {
                var winDialog = dialog.GetParentWin()?.GetDialog();
                if (winDialog != null)
                {
                    dialog = winDialog;
                }
            }
            return dialog;
        }

    }
}
