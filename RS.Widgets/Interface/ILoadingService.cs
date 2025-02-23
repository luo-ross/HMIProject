using NPOI.SS.Formula.Functions;
using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Interface
{
    internal interface ILoadingService
    {
        /// <summary>
        /// 显示加载
        /// </summary>
        void ShowLoading();

        /// <summary>
        /// 隐藏加载
        /// </summary>
        void HideLoading();

        /// <summary>
        /// 设置加载提示语
        /// </summary>
        /// <param name="loadingText"></param>
        void SetLoadingText(string loadingText);

        /// <summary>
        /// 加载异步事件
        /// </summary>
        /// <param name="func">待返回的委托</param>
        /// <param name="modelBase">实体基类</param>
        /// <param name="isLoadingValid">是否加载验证</param>
        /// <param name="isShowDialog">是否自动显示提示框</param>
        Task<OperateResult> LoadingInvokeAsync(Func<Task<OperateResult>> func,LoadingInvokeConfig loadingInvokeConfig);


        /// <summary>
        /// 加载异步事件
        /// </summary>
        /// <param name="func">待返回的委托</param>
        /// <param name="modelBase">实体基类</param>
        /// <param name="isLoadingValid">是否加载验证</param>
        /// <param name="isShowDialog">是否自动显示提示框</param>
        Task<OperateResult<T>> LoadingInvokeAsync<T>(Func<Task<OperateResult<T>>> func, LoadingInvokeConfig loadingInvokeConfig);
    }
}
