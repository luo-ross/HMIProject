using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interface
{
    public interface IModal
    {
        void ShowModal(object content);
        void HideModal();
    }
}
