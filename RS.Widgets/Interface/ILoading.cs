using RS.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interface
{
    public interface ILoading
    {
        Task<OperateResult> InvokeLoadingActionAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default);
        Task<OperateResult<T>> InvokeLoadingActionAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default);
    }
}
