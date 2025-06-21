using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Interfaces
{
    public interface IDialogManager
    {
        void RegisterDialog( string dialogKey, IDialog dialog);

        void UnregisterDialog(string dialogKey);

        IDialog GetDialog( string dialogKey);

        Task RunWithLoading(string dialogKey, Func<Task> action, LoadingConfig loadingConfig);

        void ShowWinMessage(string msg);
        void ShowWinModal(object content);
    }
}
