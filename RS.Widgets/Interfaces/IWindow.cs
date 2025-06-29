using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IWindow : IDialogBase, ILoading, IMessage, IInfoBar, IModal, IWinModal
    {
        IDialog Dialog { get; }
        IWinMessage WinMessageBox { get; }
    }
}
