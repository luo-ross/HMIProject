using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class LoginViewModel : ViewModelBase
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;

        /// <summary>
        /// 获取图像验证码回调
        /// </summary>
        public Func<Task<OperateResult<ImgVerifyResultModel>>> GetImgVerifyResultAsyncCallBack { get; set; }

        /// <summary>
        /// 关闭窗体事件回调
        /// </summary>
        public Func<OperateResult> CloseWindowCallBack { get; set; }

        /// <summary>
        /// 登录
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// 忘记密码
        /// </summary>
        public ICommand ForgetPasswordCommand { get; }

        /// <summary>
        /// 注册
        /// </summary>
        public ICommand RegisterCommand { get; }


        /// <summary>
        /// 取消二维码登录
        /// </summary>
        public Action<QRCodeLoginResultModel> CancelQRCodeLoginCommand { get; }

        /// <summary>
        /// 获取登录二维码
        /// </summary>
        public Func<Task<QRCodeLoginResultModel>> GetLoginQRCodeCommand { get; }

        /// <summary>
        /// 二维码授权登录成功
        /// </summary>
        public Action<QRCodeLoginResultModel> QRCodeAuthLoginSuccessCommand { get; }

        /// <summary>
        /// 查询二维码登录状态
        /// </summary>
        public Func<Task<QRCodeLoginStatusModel>> QueryQRCodeLoginStatusCommand { get; }

        /// <summary>
        /// 初始化验证码事件
        /// </summary>
        public Func<Task<OperateResult<ImgVerifyModel>>> InitVerifyControlAsyncCommand { get; }

        /// <summary>
        /// 验证码拖拽开始事件
        /// </summary>
        public Func<OperateResult> BtnSliderDragStartedCommand { get; }

        public LoginViewModel(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL)
        {
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.LoginCommand = new RelayCommand(LoginClick);
            this.ForgetPasswordCommand = new RelayCommand(ForgetPasswordClick);
            this.RegisterCommand = new RelayCommand(RegisterClick);
            this.CancelQRCodeLoginCommand = OnCancelQRCodeLogin;
            this.GetLoginQRCodeCommand = OnGetLoginQRCode;
            this.QRCodeAuthLoginSuccessCommand = OnQRCodeAuthLoginSuccess;
            this.QueryQRCodeLoginStatusCommand = OnQueryQRCodeLoginStatus;
            this.InitVerifyControlAsyncCommand = OnInitVerifyControlAsync;
            this.BtnSliderDragStartedCommand = OnBtnSliderDragStarted;
        }

        private OperateResult OnBtnSliderDragStarted()
        {
            // 验证用户名和输入密码是否符合要求
            var validResult = this.LoginModel.ValidObject();
            if (!validResult)
            {
                return OperateResult.CreateFailResult();
            }
            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 请求获取滑动图像验证数据
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult<ImgVerifyModel>> OnInitVerifyControlAsync()
        {
            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.ProgressBar,
                //Minimum = 0,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true,
            };

            var getImgVerifyModelResult = await this.Loading.InvokeAsync<ImgVerifyModel>(async (cancellationToken) =>
            {
                //await Task.Delay(2000);
                return await RSAppAPI.Security.GetImgVerifyModel.AESHttpGetAsync<ImgVerifyModel>(nameof(RSAppAPI));
            }, loadingConfig);

            if (!getImgVerifyModelResult.IsSuccess)
            {
                this.ParentWin.ShowInfoAsync(getImgVerifyModelResult.Message, InfoType.Error);
            }

            return getImgVerifyModelResult;
        }


        private async Task<QRCodeLoginStatusModel> OnQueryQRCodeLoginStatus()
        {
            var operateResult = await this.Loading.InvokeAsync<QRCodeLoginStatusModel>(async (cancellationToken) =>
            {

                return OperateResult.CreateSuccessResult(new QRCodeLoginStatusModel());
            });

            if (!operateResult.IsSuccess)
            {
                this.ParentWin.ShowInfoAsync(operateResult.Message, InfoType.Error);
            }
            return operateResult.Data;
        }


        private void OnQRCodeAuthLoginSuccess(QRCodeLoginResultModel model)
        {
            this.Loading.InvokeAsync(async (cancellationToken) =>
             {

                 //待实现
                 return OperateResult.CreateSuccessResult();
             });
        }


        private async Task<QRCodeLoginResultModel> OnGetLoginQRCode()
        {
            var operateResult = await this.Loading.InvokeAsync<QRCodeLoginResultModel>(async (cancellationToken) =>
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
                return OperateResult.CreateSuccessResult(loginQRCodeResultModel);
            });

            if (!operateResult.IsSuccess)
            {
                this.ParentWin.ShowInfoAsync(operateResult.Message, InfoType.Error);
            }
            return operateResult.Data;
        }

        private void OnCancelQRCodeLogin(QRCodeLoginResultModel model)
        {

        }



        /// <summary>
        /// 注册按钮点击事件
        /// </summary>
        private void RegisterClick()
        {
            this.RegisterView = App.ServiceProvider.GetService<RegisterView>();
            if (this.RegisterView == null)
            {
                return;
            }
            this.RegisterView.OnBtnReturnClick += RegisterView_OnBtnReturnClick;

            this.PasswordLoginAreaVisibility = Visibility.Collapsed;
        }

        private void RegisterView_OnBtnReturnClick()
        {
            if (this.RegisterView == null)
            {
                return;
            }
            this.RegisterView.OnBtnReturnClick -= RegisterView_OnBtnReturnClick;
            this.RegisterView = null;
            this.PasswordLoginAreaVisibility = Visibility.Visible;
        }

        /// <summary>
        /// 忘记密码点击事件
        /// </summary>
        private void ForgetPasswordClick()
        {
            //这里每次都需要重新获取服务
            this.SecurityView = App.ServiceProvider?.GetService<SecurityView>();
            if (this.SecurityView == null)
            {
                return;
            }
            this.SecurityView.OnBtnReturnClick += SecurityView_OnBtnReturnClick;
            this.PasswordLoginAreaVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 更改密码返回按钮点击事件
        /// </summary>
        private void SecurityView_OnBtnReturnClick()
        {
            if (this.SecurityView == null)
            {
                return;
            }
            this.SecurityView.OnBtnReturnClick -= SecurityView_OnBtnReturnClick;
            this.SecurityView = null;
            this.PasswordLoginAreaVisibility = Visibility.Visible;
        }



        /// <summary>
        /// 登录点击事件
        /// </summary>
        private async void LoginClick()
        {


            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.ProgressBar,
                //Minimum = 0,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true,
                //IconWidth = 25,
                //IconHeight = 32,
                //LoadingBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88000000")),
                //LoadingColor = new SolidColorBrush(Colors.Red),
                //IconData = Geometry.Parse("M512 973.653333a34.133333 34.133333 0 0 1-24.132267-58.2656l27.050667-27.067733h-3.771733c-102.8096 0-191.8976-36.949333-264.772267-109.841067S136.533333 616.209067 136.533333 512.853333c0-69.6832 17.134933-133.597867 50.944-189.934933a34.133333 34.133333 0 0 1 58.538667 35.1232C218.658133 403.626667 204.8 455.714133 204.8 512.853333c0 85.794133 29.3888 156.893867 89.838933 217.361067 60.450133 60.450133 131.2768 89.838933 216.507734 89.838933h3.771733l-27.050667-27.067733a34.133333 34.133333 0 1 1 48.264534-48.264533l85.282133 85.282133 0.529067 0.529067c3.0208 3.140267 5.307733 6.7072 6.877866 10.496a34.030933 34.030933 0 0 1-7.355733 37.290666l-85.333333 85.333334c-6.673067 6.656-15.394133 10.001067-24.132267 10.001066z m297.7792-257.706666a34.1504 34.1504 0 0 1-29.508267-51.2512C806.0928 620.1344 819.2 569.0368 819.2 512.853333c0-85.794133-29.3888-156.893867-89.838933-217.361066-60.4672-60.450133-131.293867-89.838933-216.541867-89.838934h-3.191467l27.221334 26.948267a34.133333 34.133333 0 0 1-48.0256 48.520533l-86.186667-85.333333-0.8704-0.887467-0.017067-0.034133-0.034133-0.034133a34.133333 34.133333 0 0 1 0.8704-47.496534l86.135467-86.1184a34.133333 34.133333 0 1 1 48.264533 48.264534L509.0816 137.386667h3.754667c102.8096 0 191.914667 36.9664 264.8064 109.841066S887.466667 409.480533 887.466667 512.853333c0 68.369067-16.1792 130.9696-48.110934 186.077867a34.116267 34.116267 0 0 1-29.576533 17.015467z")
            };

            var operateResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            {
                // 验证用户名和输入密码是否符合要求
                var validResult = this.LoginModel.ValidObject();
                if (!validResult)
                {
                    return OperateResult.CreateFailResult("数据验证失败！");
                }

                OperateResult<ImgVerifyResultModel>? getImgVerifyResult = null;
                if (this.GetImgVerifyResultAsyncCallBack != null)
                {
                    //获取验证码
                    getImgVerifyResult = await this.GetImgVerifyResultAsyncCallBack.Invoke();
                }

                if (getImgVerifyResult == null)
                {
                    return OperateResult.CreateFailResult("获取验证码失败！");
                }

                if (!getImgVerifyResult.IsSuccess)
                {
                    return getImgVerifyResult;
                }

                var imgVerifyResultModel = getImgVerifyResult.Data;

                //验证用户登录
                var validLoginResult = await RSAppAPI.Security.ValidLogin.AESHttpPostAsync(new LoginValidModel()
                {
                    Email = this.LoginModel.Email,
                    Password = this.CryptographyBLL.GetSHA256HashCode(this.LoginModel.Password),
                    Verify = imgVerifyResultModel.Verify,
                    VerifySessionId = imgVerifyResultModel.VerifySessionId,
                }, nameof(RSAppAPI));

                if (!validLoginResult.IsSuccess)
                {
                    return validLoginResult;
                }

                return OperateResult.CreateSuccessResult();
            }, loadingConfig);


            //如果验证成功
            if (!operateResult.IsSuccess)
            {
                this.ParentWin.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }

            operateResult = this.CloseWindowCallBack?.Invoke();
            if (operateResult == null)
            {
                this.ParentWin.ShowInfoAsync("登录窗体事件未实现！", InfoType.Warning);
                return;
            }
            if (!operateResult.IsSuccess)
            {
                this.ParentWin.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }

            var homeView = App.ServiceProvider?.GetService<HomeView>();
            homeView?.Show();
        }

        private RegisterView registerView;
        /// <summary>
        /// 注册窗体
        /// </summary>
        public RegisterView RegisterView
        {
            get
            {
                return registerView;
            }
            set
            {
                this.SetProperty(ref registerView, value);
            }
        }

        private SecurityView securityView;
        /// <summary>
        /// 更改密码窗体
        /// </summary>
        public SecurityView SecurityView
        {
            get
            {
                return securityView;
            }
            set
            {
                this.SetProperty(ref securityView, value);
            }
        }


        private LoginModel loginModel;
        /// <summary>
        /// 登录实体
        /// </summary>
        public LoginModel LoginModel
        {
            get
            {
                if (loginModel == null)
                {
                    loginModel = new LoginModel();
                }
                return loginModel;
            }
            set
            {
                this.SetProperty(ref loginModel, value);
            }
        }


        private SignUpModel signUpModel;
        /// <summary>
        /// 注册
        /// </summary>
        public SignUpModel SignUpModel
        {
            get
            {
                if (signUpModel == null)
                {
                    signUpModel = new SignUpModel();
                }
                return signUpModel;
            }
            set
            {
                this.SetProperty(ref signUpModel, value);
            }
        }


        private Visibility passwordLoginAreaVisibility = Visibility.Visible;
        public Visibility PasswordLoginAreaVisibility
        {
            get
            {
                return passwordLoginAreaVisibility;
            }
            set
            {
                this.SetProperty(ref passwordLoginAreaVisibility, value);
            }
        }


    }
}
