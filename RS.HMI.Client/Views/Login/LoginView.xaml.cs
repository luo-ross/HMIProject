using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Diagnostics;
using System.Windows;

namespace RS.HMI.Client.Views
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class LoginView : RSWindow
    {
        /// <summary>
        /// 数据实体
        /// </summary>
        private readonly LoginViewModel ViewModel;


        /// <summary>
        /// 默认构造方法
        /// </summary>
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.ViewModel = viewModel;
            this.ViewModel.GetImgVerifyResultAsyncCallBack = ViewModel_GetImgVerifyResultAsyncCallBack;
            this.ViewModel.CloseWindowCallBack = ViewModel_CloseWindowCallBack;
        }

        private OperateResult ViewModel_CloseWindowCallBack()
        {
            this.Close();
            return OperateResult.CreateSuccessResult();
        }

        private Task<OperateResult<ImgVerifyResultModel>> ViewModel_GetImgVerifyResultAsyncCallBack()
        {
            return Task.FromResult(this.ImgVerify.GetImgVerifyResultAsync());
        }

        /// <summary>
        /// 超级连接跳转
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", e.Uri.ToString());
        }

    }
}
