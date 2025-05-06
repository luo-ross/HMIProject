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
using System;
using System.Collections.Generic;
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

namespace RS.HMI.Client.Views
{
    /// <summary>
    /// RegisterView.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterView : RSUserControl
    {
        private readonly RegisterViewModel ViewModel;
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        public RegisterView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as RegisterViewModel;
            if (App.AppHost != null)
            {
                this.GeneralBLL = App.AppHost.Services.GetRequiredService<IGeneralBLL>();
                this.CryptographyBLL = App.AppHost.Services.GetRequiredService<ICryptographyBLL>();
            }
        }

        public event Func<RSUserControl> GetLoadingControl;
        private async void BtnSignUpNext_Click(object sender, RoutedEventArgs e)
        {
            //注册信息验证
            var validResult = this.ViewModel.SignUpModel.ValidObject();
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

            //获取自定义
            var loading = GetLoadingControl?.Invoke();
            if (loading == null)
            {
                loading = this;
            }

            var operateResult = await loading.InvokeLoadingActionAsync(async () =>
            {
                var emailRegisterPostModel = new EmailRegisterPostModel();
                emailRegisterPostModel.Email = this.ViewModel.SignUpModel.UserName;
                emailRegisterPostModel.Password = this.CryptographyBLL.GetSHA256HashCode(this.ViewModel.SignUpModel.PasswordConfirm);

                //获取邮箱验证码结果
                var getEmailVerifyResult = await RSAppAPI.Register.GetEmailVerify.AESHttpPostAsync<EmailRegisterPostModel, RegisterVerifyModel>(nameof(RSAppAPI), emailRegisterPostModel);

                if (!getEmailVerifyResult.IsSuccess)
                {
                    return getEmailVerifyResult;
                }

                return getEmailVerifyResult;
            }, loadingConfig);

            //如果失败
            if (!operateResult.IsSuccess)
            {
                this.TryFindParent<RSWindow>()?.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }
        }
    }
}
