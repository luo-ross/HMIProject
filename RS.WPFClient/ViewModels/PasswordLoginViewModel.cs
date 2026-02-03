using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Models;
using RS.Server.WebAPI;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using RS.WPFClient.IBLL;
using RS.WPFClient.IServices;
using RS.WPFClient.Models;
using System.Windows.Input;

namespace RS.WPFClient.ViewModels
{
    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public class PasswordLoginViewModel : ViewModelBase
    {
        #region 依赖注入服务
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
      
        #endregion

        #region 自定义事件
        public event Action OnRegister;
        public event Action OnForgetPassword;
        public event Action OnQRLogin;
        public event Action OnLoinSuccess;
        #endregion

        #region 命令
        /// <summary>
        /// 登录
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// 注册
        /// </summary>
        public ICommand RegisterCommand { get; }

        /// <summary>
        /// 忘记密码
        /// </summary>
        public ICommand ForgetPasswordCommand { get; }

        public ICommand QRLoginCommand { get; }
        #endregion

        public PasswordLoginViewModel(IGeneralBLL generalBLL,
            ICryptographyBLL cryptographyBLL)
        {
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.LoginCommand = new RelayCommand(Login);
            this.ForgetPasswordCommand = new RelayCommand(ForgetPassword);
            this.RegisterCommand = new RelayCommand(Register);
            this.QRLoginCommand = new RelayCommand(QRLogin);
        }




        private LoginModel? loginModel;
        /// <summary>
        /// 登录实体
        /// </summary>
        public LoginModel? LoginModel
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
                SetProperty(ref loginModel, value);
            }
        }


        private IImgVerifyService? imgVerifyService;
        /// <summary>
        /// 注册服务
        /// </summary>
        public IImgVerifyService? ImgVerifyService
        {
            get
            {
                return imgVerifyService;
            }
            set
            {
                SetProperty(ref imgVerifyService, value);
                if (imgVerifyService != null)
                {
                    imgVerifyService.SliderDragStarted += ImgVerifyService_SliderDragStarted;
                    imgVerifyService.InitVerifyControlAsync += ImgVerifyService_InitVerifyControlAsync;
                }
            }
        }


        private void QRLogin()
        {
            OnQRLogin?.Invoke();
        }

        /// <summary>
        /// 注册按钮点击事件
        /// </summary>
        private void Register()
        {
            OnRegister?.Invoke();
        }

        /// <summary>
        /// 忘记密码点击事件
        /// </summary>
        private void ForgetPassword()
        {
            OnForgetPassword?.Invoke();
        }


        /// <summary>
        /// 登录点击事件
        /// </summary>
        private async void Login()
        {
            if (ImgVerifyService == null)
            {
                await ShowMessageAsync($"{nameof(ImgVerifyService)} not register");
                return;
            }

            //这里这个登录验证先跳过 后面慢慢实现

            //var loadingConfig = new LoadingConfig()
            //{
            //    LoadingType = LoadingType.ProgressBar,
            //    //Minimum = 0,
            //    Maximum = 100,
            //    Value = 0,
            //    IsIndeterminate = true,
            //    //IconWidth = 25,
            //    //IconHeight = 32,
            //    //LoadingBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88000000")),
            //    //LoadingColor = new SolidColorBrush(Colors.Red),
            //    //IconData = Geometry.Parse("M512 973.653333a34.133333 34.133333 0 0 1-24.132267-58.2656l27.050667-27.067733h-3.771733c-102.8096 0-191.8976-36.949333-264.772267-109.841067S136.533333 616.209067 136.533333 512.853333c0-69.6832 17.134933-133.597867 50.944-189.934933a34.133333 34.133333 0 0 1 58.538667 35.1232C218.658133 403.626667 204.8 455.714133 204.8 512.853333c0 85.794133 29.3888 156.893867 89.838933 217.361067 60.450133 60.450133 131.2768 89.838933 216.507734 89.838933h3.771733l-27.050667-27.067733a34.133333 34.133333 0 1 1 48.264534-48.264533l85.282133 85.282133 0.529067 0.529067c3.0208 3.140267 5.307733 6.7072 6.877866 10.496a34.030933 34.030933 0 0 1-7.355733 37.290666l-85.333333 85.333334c-6.673067 6.656-15.394133 10.001067-24.132267 10.001066z m297.7792-257.706666a34.1504 34.1504 0 0 1-29.508267-51.2512C806.0928 620.1344 819.2 569.0368 819.2 512.853333c0-85.794133-29.3888-156.893867-89.838933-217.361066-60.4672-60.450133-131.293867-89.838933-216.541867-89.838934h-3.191467l27.221334 26.948267a34.133333 34.133333 0 0 1-48.0256 48.520533l-86.186667-85.333333-0.8704-0.887467-0.017067-0.034133-0.034133-0.034133a34.133333 34.133333 0 0 1 0.8704-47.496534l86.135467-86.1184a34.133333 34.133333 0 1 1 48.264533 48.264534L509.0816 137.386667h3.754667c102.8096 0 191.914667 36.9664 264.8064 109.841066S887.466667 409.480533 887.466667 512.853333c0 68.369067-16.1792 130.9696-48.110934 186.077867a34.116267 34.116267 0 0 1-29.576533 17.015467z")
            //};

            //var operateResult = await Loading.InvokeAsync(async (cancellationToken) =>
            //   {
            //       // 验证用户名和输入密码是否符合要求
            //       var validResult = this.ValidForm();
            //       if (!validResult.IsSuccess)
            //       {

            //           return validResult;
            //       }

            //       //获取验证码
            //       OperateResult<ImgVerifyResultModel> getImgVerifyResult = await ImgVerifyService.GetImgVerifyResultAsync();

            //       if (getImgVerifyResult == null || !getImgVerifyResult.IsSuccess)
            //       {
            //           await ImgVerifyService.ResetImgVerifyAsync();
            //           await ShowMessageAsync(getImgVerifyResult.Message);
            //           return getImgVerifyResult;
            //       }

            //       var imgVerifyResultModel = getImgVerifyResult.Data;
            //       //验证用户登录
            //       var validLoginResult = await HMIWebAPI.Security.ValidLogin.AESHttpPostAsync(new LoginValidModel()
            //       {
            //           Email = LoginModel.Email,
            //           Password = CryptographyBLL.GetSHA256HashCode(LoginModel.Password),
            //           Verify = imgVerifyResultModel.Verify,
            //           VerifySessionId = imgVerifyResultModel.VerifySessionId,
            //       }, nameof(HMIWebAPI));
            //       return validLoginResult;
            //   }, loadingConfig);
            //if (!operateResult.IsSuccess)
            //{
            //    return;
            //}

            OnLoinSuccess?.Invoke();

        }



        /// <summary>
        /// 请求获取滑动图像验证数据
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult<ImgVerifyModel>> ImgVerifyService_InitVerifyControlAsync()
        {
            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.ProgressBar,
                //Minimum = 0,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true,
            };

            var getImgVerifyModelResult = await Loading.InvokeAsync(async (cancellationToken) =>
            {
                return await HMIWebAPI.Security.GetImgVerifyModel.AESHttpGetAsync<ImgVerifyModel>(nameof(HMIWebAPI));
            }, loadingConfig);

            if (!getImgVerifyModelResult.IsSuccess)
            {
                ParentWin.ShowInfoAsync(getImgVerifyModelResult.Message, InfoType.Error);
            }

            return getImgVerifyModelResult;
        }

        private OperateResult ImgVerifyService_SliderDragStarted()
        {
            return ValidForm();
        }

        private OperateResult ValidForm()
        {
            // ValidationFocusBehavior 会自动在验证失败时设置焦点
            if (!this.LoginModel.ValidObject())
            {
                return OperateResult.CreateFailResult();
            }
            return OperateResult.CreateSuccessResult();
        }
    }
}
