using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extend;
using RS.Commons.Extensions;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RS.HMI.Client.Views
{
    /// <summary>
    /// 采用依赖注入使用的时候获取服务
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public partial class SecurityView : RSDialog
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly SecurityViewModel ViewModel;
        #region 自定义事件
        public event Action OnBtnReturnClick;
        #endregion
        /// <summary>
        /// 依赖注入 构造函数
        /// </summary>
        public SecurityView(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL)
        {
            InitializeComponent();
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.ViewModel = this.DataContext as SecurityViewModel;
        }
     

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.OnBtnReturnClick?.Invoke();
        }

        private async void BtnSendEmailPasswordReset_Click(object sender, RoutedEventArgs e)
        {
            // 验证用户名和输入密码是否符合要求
            var validResult = this.ViewModel.ValidObject();
            if (!validResult)
            {
                return;
            }

            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.ProgressBar,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true,
            };

            var operateResult = await this.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                var emailModel = new EmailModel();
                emailModel.Email = this.ViewModel.UserName;
                //获取邮箱验证码结果
                var passwordResetEmailSendResult = await RSAppAPI.Security.PasswordResetEmailSend.AESHttpPostAsync<EmailModel>(emailModel, nameof(RSAppAPI));
                if (!passwordResetEmailSendResult.IsSuccess)
                {
                    return passwordResetEmailSendResult;
                }
                return passwordResetEmailSendResult;
            }, loadingConfig);

            //如果失败
            if (!operateResult.IsSuccess)
            {
                this.TryFindParent<RSWindow>()?.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }

            //如果注册成功 跳转到注册成功页面
            this.PART_EmailSendSuccess.Visibility = Visibility.Visible;
        }

        private void BtnReturnLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.OnBtnReturnClick?.Invoke();
        }

    
    }


}
