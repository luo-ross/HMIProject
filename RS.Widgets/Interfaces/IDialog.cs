using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IDialog : IDialogBase, ILoading, IMessage, IModal, IWinModal
    {
        IWindow ParentWin { get; }

        ILoading RootLoading { get; }

        IModal RootModal { get; }

        IMessage RootMessageBox { get; }

        INavigate Navigate { get; }
    }
}
