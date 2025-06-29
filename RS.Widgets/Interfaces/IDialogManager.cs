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
        void RegisterDialog(object dialogKey, IDialog dialog);

         void UnregisterDialog(object dialogKey);

         IDialog GetDialog(object dialogKey);
       
    }
}
