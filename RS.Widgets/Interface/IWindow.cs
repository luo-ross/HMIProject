using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interface
{
    public interface IWindow : IInfoBar
    {
        IModal GetModal();
        ILoading GetLoading();

        IDialog GetDialog();

        IMessage GetMessageBox();

        IMessage GetWinMessageBox();
    }
}
