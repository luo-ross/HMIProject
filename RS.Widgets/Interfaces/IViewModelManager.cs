using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interfaces
{
    public interface IViewModelManager
    {

        Type? GetViewModelType(string viewKey);

        object? GetViewModel(string viewKey);

        T? GetViewModel<T>(string viewKey) where T : INotifyPropertyChanged;

    }
}
