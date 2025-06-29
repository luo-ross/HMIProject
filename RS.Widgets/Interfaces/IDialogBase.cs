using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IDialogBase : ILoading, IModal, IMessage, IWinModal
    {
        ILoading Loading { get; }

        IModal Modal { get; }

        IWinModal WinModal { get; }

        IMessage MessageBox { get; }
    }
}
