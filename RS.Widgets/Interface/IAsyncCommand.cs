using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RS.Widgets.Interface
{
    public interface IAsyncCommand<TResult>
    {
        event EventHandler? CanExecuteChanged;
    
        Task<bool> CanExecuteAsync(object? parameter);

        Task<TResult> ExecuteAsync(object? parameter);
    }
}
