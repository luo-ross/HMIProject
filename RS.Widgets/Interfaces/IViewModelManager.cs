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

        Type? GetViewModelType(string viewModelKey);

        object? GetViewModel(string viewModelKey);

        T? GetViewModel<T>(string viewModelKey) where T : INotifyPropertyChanged;

    }
}
