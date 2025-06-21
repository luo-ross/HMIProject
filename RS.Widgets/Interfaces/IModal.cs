using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IModal
    {
        void ShowModal(object content);
        void HideModal();
    }
}
