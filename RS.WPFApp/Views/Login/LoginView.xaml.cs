using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using RS.WPFApp.IBLL;
using RS.WPFApp.Views.Home;
using System.Diagnostics;
using System.Windows;

namespace RS.WPFApp.Views
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class LoginView : RSWindow
    {
        public RSDesktop RSDesktop { get; set; }
        private readonly IGeneralService GeneralService;
        private readonly ICryptographyService CryptographyService;
        private readonly LoginViewModel ViewModel;
        public LoginView(IGeneralService generalService, ICryptographyService cryptographyService)
        {
            InitializeComponent();
            this.GeneralService = generalService;
            this.CryptographyService = cryptographyService;
            this.ViewModel = this.DataContext as LoginViewModel;
            this.Closed += LoginView_Closed;
            this.Loaded += RSWindow_Loaded;
            //this.IsCloseBtnVisible = Visibility.Collapsed;
        }

        private async void RSWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadingBg.Visibility = Visibility.Visible;
            Task.Factory.StartNew(async () =>
            {
                var getSessionModelResult = await this.GeneralService.GetSessionModelAsync();
                if (!getSessionModelResult.IsSuccess)
                {
                    this.Dispatcher.Invoke(async () =>
                    {
                        await this.MessageBox.ShowAsync(getSessionModelResult.Message);
                    });
                }
                this.Dispatcher.Invoke(() =>
                {
                    this.LoadingBg.Visibility = Visibility.Collapsed;
                    //this.IsCloseBtnVisible = Visibility.Visible;
                });

            });
        }

        private void LoginView_Closed(object? sender, EventArgs e)
        {
            this.RSDesktop?.Close();
        }

        private async Task<VerificationModel> TxtVerification_GetVerificationClick()
        {
            //用户就在这里去往服务端发起请求获取验证码
            var expireTime = new DateTimeOffset(DateTime.Now.AddSeconds(60)).ToUnixTimeMilliseconds();
            VerificationModel verificationResultModel = new VerificationModel()
            {
                IsSuccess = true,
                ExpireTime = expireTime
            };
            return verificationResultModel;
        }

        private async Task<QRCodeLoginResultModel> QRCodeLogin_OnGetLoginQRCode()
        {
            return await Task.Factory.StartNew(() =>
            {
                //用户就在这里去往服务端发起请求获取验证码
                var expireTime = new DateTimeOffset(DateTime.Now.AddSeconds(120)).ToUnixTimeMilliseconds();
                //https://passport.iqiyi.com/apis/qrcode/token_login.action?token=7a068e22fe923ea273bcf76242db4bfba
                string token = $"{Guid.NewGuid().ToString()}";
                QRCodeLoginResultModel loginQRCodeResultModel = new QRCodeLoginResultModel()
                {
                    IsSuccess = true,
                    Token = token,
                    QRCodeContent = $"https://passport.myweb.com/apis/qrcode/token_login?token={token}",
                    ExpireTime = expireTime
                };
                return loginQRCodeResultModel;
            });
        }

        private async Task<QRCodeLoginStatusModel> QRCodeLogin_OnQueryQRCodeLoginStatus()
        {
            QRCodeLoginStatusModel qRCodeLoginStatusModel = new QRCodeLoginStatusModel()
            {
                IsSuccess = true,
                QRCodeLoginStatus = QRCodeLoginStatusEnum.WaitScanQRCode,
                Message = "等待扫描二维码"
            };

            //QRCodeLoginStatusModel qRCodeLoginStatusModel = new QRCodeLoginStatusModel()
            //{
            //    IsSuccess = true,
            //    QRCodeLoginStatus = QRCodeLoginStatusEnum.ScanQRCodeSuccess,
            //    Message = "扫码成功"
            //};


            //QRCodeLoginStatusModel qRCodeLoginStatusModel = new QRCodeLoginStatusModel()
            //{
            //    IsSuccess = true,
            //    QRCodeLoginStatus = QRCodeLoginStatusEnum.QRCodeAuthLogin,
            //    Message = "成功授权登录"
            //};

            return qRCodeLoginStatusModel;
        }

        private void QRCodeLogin_OnQRCodeAuthLoginSuccess(QRCodeLoginResultModel loginQRCodeResult)
        {
            //出发授权登录成功回调 自己再去重新请求获取服务端用户的信息
            var sdf = 1;
        }

        private void QRCodeLogin_OnCancelQRCodeLogin(QRCodeLoginResultModel loginQRCodeResult)
        {
            //加入取消二维码登录 向服务发送信息 取消二维码登录 主动删除服务端数据
            var sdf = 1;
        }





        private void ShowRSDesktop(string sourcePath)
        {
            if (this.RSDesktop != null)
            {
                this.RSDesktop.Close();
            }
            this.RSDesktop = new RSDesktop();
            this.RSDesktop.VideoSourceUri = new Uri(sourcePath, UriKind.RelativeOrAbsolute);
            this.RSDesktop.Show();
        }

        private void BtnShark_Click(object sender, RoutedEventArgs e)
        {
            ShowRSDesktop("Assets/Videos/shark.mp4");
        }

        private void BtnGirl_Click(object sender, RoutedEventArgs e)
        {
            ShowRSDesktop("Assets/Videos/girl.mp4");
        }

        private void BtnRobot_Click(object sender, RoutedEventArgs e)
        {
            ShowRSDesktop("Assets/Videos/robot.mp4");
        }

        private void BtnOcean_Click(object sender, RoutedEventArgs e)
        {
            ShowRSDesktop("Assets/Videos/ocean.mp4");
        }

        private async void BtnPasswordLogin_Click(object sender, RoutedEventArgs e)
        {
            //验证用户名和密码数据
            var validResult = this.ViewModel.PasswordLoginModel.ValidObject();
            if (!validResult)
            {
                return;
            }

            var validLoginResult = await this.InvokeLoadingActionAsync(async () =>
            {
                //this.SetLoadingText("正在登录中...");
                //验证用户登录
                var validLoginResult = await RSAppAPI.User.ValidLogin.AESHttpPostAsync(nameof(RSAppAPI), new LoginValidModel()
                {
                    UserName = this.ViewModel.PasswordLoginModel.UserName,
                    Password = this.CryptographyService.GetSHA256HashCode(this.ViewModel.PasswordLoginModel.Password),
                });

                return validLoginResult;
            });

            //如果验证成功
            if (validLoginResult.IsSuccess)
            {
                var homeView = App.AppHost?.Services.GetService<HomeView>();
                homeView?.Show();
                this.Close();
            }
        }

        private async void BtnSMMLogin_Click(object sender, RoutedEventArgs e)
        {
            var validResult = this.ViewModel.SMSRegisterModel.ValidObject();
            if (!validResult)
            {
                return;
            }
            var smsRegisterModel = this.ViewModel.SMSRegisterModel;
        }

        /// <summary>
        /// 重置密码事件处理
        /// </summary>
        private void BtnForgetPassword_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo($"{App.AppHostAddress}/password/new");
            processStartInfo.UseShellExecute = true;
            Process.Start(processStartInfo);
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            this.RegisterHostView.Visibility = Visibility.Visible;
            RegisterViewModel registerViewModel = new RegisterViewModel();
            this.RegisterView.DataContext = registerViewModel;
            this.RegisterView.ViewModel = registerViewModel;
            this.LoginHostView.Visibility = Visibility.Collapsed;
        }



        private void RegisterView_RegisterEnd(bool isSuccess)
        {
            this.RegisterHostView.Visibility = Visibility.Collapsed;
            this.LoginHostView.Visibility = Visibility.Visible;

            //如果注册成功
            if (isSuccess)
            {
                this.RadioBtnPassword.IsChecked = true;
                this.TxtUserName.Focus();
            }
        }
    }
}
