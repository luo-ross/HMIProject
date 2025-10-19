using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Controls;
using RS.HMI.Client.Models;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;


namespace RS.HMI.Client.Views
{
    // 使用现成的 AsyncRequestMessage
    public class DataRequestMessage : AsyncRequestMessage<string>
    {
        public string Prompt { get; set; }

        public DataRequestMessage(string prompt)
        {
            Prompt = prompt;
        }
    }

    public class BoolRequestMessage : RequestMessage<bool>
    {
        public string Question { get; set; }

        public BoolRequestMessage(string question)
        {
            Question = question;
        }
    }

    public class CalculateRequestMessage : AsyncRequestMessage<int>
    {
        public int Value { get; set; }

        public CalculateRequestMessage(int value)
        {
            Value = value;
        }
    }



    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class LoginViewModel : ViewModelBase
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private RegisterView RegisterView;
        private SecurityView SecurityView;
        public ICommand RegisterCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand ForgetPasswordCommand { get; set; }


        public LoginViewModel(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL)
        {
            this.RegisterCommand = new RelayCommand(RegisterClick, CanRegisterClick);
            this.LoginCommand = new RelayCommand(LoginClick, CanLoginClick);
            this.ForgetPasswordCommand = new RelayCommand(ForgetPasswordClick, CanForgetPasswordClick);
            this.InitVerifyControlAsync = OnInitVerifyControlAsync;
            this.SliderDragStarted = OnSliderDragStarted;

            this.GetLoginQRCode = OnGetLoginQRCode;
            this.QueryQRCodeLoginStatus = OnQueryQRCodeLoginStatus;
            this.QRCodeAuthLoginSuccess = OnQRCodeAuthLoginSuccess;
            this.CancelQRCodeLogin = OnCancelQRCodeLogin;
        }


        private Func<Task<OperateResult<ImgVerifyModel>>> initVerifyControlAsync;

        public Func<Task<OperateResult<ImgVerifyModel>>> InitVerifyControlAsync
        {
            get { return initVerifyControlAsync; }
            set
            {
                this.SetProperty(ref initVerifyControlAsync, value);
            }
        }



        private Func<OperateResult> sliderDragStarted;

        public Func<OperateResult> SliderDragStarted
        {
            get { return sliderDragStarted; }
            set
            {
                this.SetProperty(ref sliderDragStarted, value);
            }
        }



        private Func<Task<QRCodeLoginResultModel>> getLoginQRCode;

        public Func<Task<QRCodeLoginResultModel>> GetLoginQRCode
        {
            get { return getLoginQRCode; }
            set
            {
                this.SetProperty(ref getLoginQRCode, value);
            }
        }



        private Func<Task<QRCodeLoginStatusModel>> queryQRCodeLoginStatus;

        public Func<Task<QRCodeLoginStatusModel>> QueryQRCodeLoginStatus
        {
            get { return queryQRCodeLoginStatus; }
            set
            {
                this.SetProperty(ref queryQRCodeLoginStatus, value);
            }
        }

        private Action<QRCodeLoginResultModel> _QRCodeAuthLoginSuccess;

        public Action<QRCodeLoginResultModel> QRCodeAuthLoginSuccess
        {
            get { return _QRCodeAuthLoginSuccess; }
            set
            {
                this.SetProperty(ref _QRCodeAuthLoginSuccess, value);
            }
        }


        private Action<QRCodeLoginResultModel> cancelQRCodeLogin;

        public Action<QRCodeLoginResultModel> CancelQRCodeLogin
        {
            get { return cancelQRCodeLogin; }
            set
            {
                this.SetProperty(ref cancelQRCodeLogin, value);
            }
        }



        private bool CanForgetPasswordClick()
        {
            return true;
        }

        private void ForgetPasswordClick()
        {
            ////这里每次都需要重新获取服务
            //this.SecurityView = App.ServiceProvider?.GetService<SecurityView>();
            //this.SecurityView.OnBtnReturnClick += SecurityView_OnBtnReturnClick;
            //this.PART_ContentHost.Children.Add(this.SecurityView);
        }

        private bool CanLoginClick()
        {
            return true;
        }
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private async void LoginClick()
        {

            // 通过消息请求数据
            var message = new BoolRequestMessage(false);

            var msg =await WeakReferenceMessenger.Default.Send(message, "LoginView");
            var sdf = 1;

            //this.InfoBar.ShowInfoAsync($"这是消息触发1，5秒后自动消失", InfoType.Warning);
            //var result = await this.RootMessageBox.ShowMessageAsync("代码解释\r\n数据准备：创建一个 Person 对象的列表，并将其设置为 ItemsControl 的 ItemsSource。\r\n遍历项容器：使用 for 循环遍历 ItemsControl 的所有项，通过 ItemContainerGenerator.ContainerFromIndex 方法获取每个项对应的 ContentPresenter 容器。\r\n查找模板内的控件：定义 FindVisualChild 方法，使用 VisualTreeHelper 递归查找容器内指定名称的 TextBlock 控件。\r\n处理控件内容：若找到控件，则输出其文本内容。\r\n通过以上步骤，你就可以在后台循环遍历 ItemsControl 下的每一个模板内容，并对其中的控件进行操作。\r\nSystem.InvalidOperationException:“在显示 Window 或调用 WindowInteropHelper.EnsureHandle 之后，无法更改 AllowsTransparency。”\r\n这个 System.InvalidOperationException 异常提示表明，在窗口已经显示或者调用了 WindowInteropHelper.EnsureHandle 方法之后，你尝试修改 AllowsTransparency 属性，而 WPF 不允许在这种情况下更改该属性。下面为你详细解释问题原因并提供解决方案。\r\n问题原因\r\nAllowsTransparency 属性用于指定窗口是否允许透明效果。在 WPF 中，窗口的句柄（Handle）一旦创建，就不能再修改 AllowsTransparency 属性，因为该属性的更改会影响窗口的样式和行为，而窗口句柄创建后其样式是固定的。窗口显示或者调用 WindowInteropHelper.EnsureHandle 方法时，窗口句柄会被创建，此时再修改 AllowsTransparency 就会抛出异常。\r\n解决方案\r\n1. 在窗口显示之前设置 AllowsTransparency\r\n确保在调用 Window.Show、Window.ShowDialog 或者窗口自动显示（如在 MainWindow 启动时）之前设置 AllowsTransparency 属性。\r\n示例代码：\r\ncsharp\r\nusing System.Windows;\r\n\r\nnamespace Annotation\r\n{\r\n    public partial class MainWindow : Window\r\n    {\r\n        public MainWindow()\r\n        {\r\n            InitializeComponent();\r\n\r\n            // 在窗口显示之前设置 AllowsTransparency\r\n            AllowsTransparency = true;\r\n            Background = System.Windows.Media.Brushes.Transparent;\r\n            WindowStyle = WindowStyle.None;\r\n        }\r\n    }\r\n}", "错误提示");
            //if (result == MessageBoxResult.OK)
            //{
            //    return;
            //}

            //// 验证用户名和输入密码是否符合要求
            //var validResult = this.LoginModel.ValidObject();
            //if (!validResult)
            //{
            //    return;
            //}

            ////获取验证码
            //var getImgVerifyResult = this.ImgVerify.GetImgVerifyResultAsync();
            //if (!getImgVerifyResult.IsSuccess)
            //{
            //    this.InfoBar.ShowInfoAsync(getImgVerifyResult.Message, InfoType.Warning);
            //    return;
            //}

            //var imgVerifyResultModel = getImgVerifyResult.Data;

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

            //var operateResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            //{
            //    //验证用户登录
            //    var validLoginResult = await RSAppAPI.Security.ValidLogin.AESHttpPostAsync(new LoginValidModel()
            //    {
            //        Email = this.LoginModel.Email,
            //        Password = this.CryptographyBLL.GetSHA256HashCode(this.LoginModel.Password),
            //        Verify = imgVerifyResultModel.Verify,
            //        VerifySessionId = imgVerifyResultModel.VerifySessionId,
            //    }, nameof(RSAppAPI));

            //    if (!validLoginResult.IsSuccess)
            //    {
            //        return validLoginResult;
            //    }

            //    await Task.Delay(5000);

            //    return OperateResult.CreateSuccessResult();
            //}, loadingConfig);


            ////如果验证成功
            //if (!operateResult.IsSuccess)
            //{
            //    this.InfoBar.ShowInfoAsync(operateResult.Message, InfoType.Warning);
            //    return;
            //}
            //var homeView = App.ServiceProvider?.GetService<HomeView>();
            //homeView?.Show();
            //this.Close();
        }

        private bool CanRegisterClick()
        {
            return true;
        }

        private void RegisterClick()
        {
            ////这里每次都需要重新获取服务
            //this.RegisterView = App.ServiceProvider?.GetService<RegisterView>();
            //this.RegisterView.OnBtnReturnClick += RegisterView_OnBtnReturnClick;
            //this.PART_ContentHost.Children.Add(this.RegisterView);
        }


        private void SecurityView_OnBtnReturnClick()
        {
            //if (this.SecurityView != null)
            //{
            //    this.SecurityView.OnBtnReturnClick -= SecurityView_OnBtnReturnClick;
            //    this.PART_ContentHost.Children.Remove(this.SecurityView);
            //}
        }



        private void RegisterView_OnBtnReturnClick()
        {
            //if (this.RegisterView != null)
            //{
            //    this.RegisterView.OnBtnReturnClick -= RegisterView_OnBtnReturnClick;
            //    this.PART_ContentHost.Children.Remove(this.RegisterView);
            //}
        }



        /// <summary>
        /// 超级连接跳转
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", e.Uri.ToString());
        }





        private void OnCancelQRCodeLogin(QRCodeLoginResultModel loginQRCodeResult)
        {

        }

        private Task<QRCodeLoginResultModel> OnGetLoginQRCode()
        {
            return Task.Factory.StartNew(() =>
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

        private void OnQRCodeAuthLoginSuccess(QRCodeLoginResultModel loginQRCodeResult)
        {

        }

        private Task<QRCodeLoginStatusModel> OnQueryQRCodeLoginStatus()
        {
            return Task.Factory.StartNew(() =>
            {
                return new QRCodeLoginStatusModel();
            });
        }

        private bool CanInitVerifyControlAsync(bool obj)
        {
            return true;
        }

        /// <summary>
        /// 请求获取滑动图像验证数据
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult<ImgVerifyModel>> OnInitVerifyControlAsync()
        {
            //var loadingConfig = new LoadingConfig()
            //{
            //    LoadingType = LoadingType.ProgressBar,
            //    //Minimum = 0,
            //    Maximum = 100,
            //    Value = 0,
            //    IsIndeterminate = true,
            //};
            //var getImgVerifyModelResult = await this.Loading.InvokeAsync<ImgVerifyModel>(async (cancellationToken) =>
            //{
            //    //await Task.Delay(2000);
            //    return await RSAppAPI.Security.GetImgVerifyModel.AESHttpGetAsync<ImgVerifyModel>(nameof(RSAppAPI));
            //}, loadingConfig);

            //if (!getImgVerifyModelResult.IsSuccess)
            //{
            //    await this.WinMessageBox.ShowMessageAsync(getImgVerifyModelResult.Message);
            //}

            //return getImgVerifyModelResult;

            return null;
        }

        private OperateResult OnSliderDragStarted()
        {
            //// 验证用户名和输入密码是否符合要求
            //var validResult = this.ViewModel.LoginModel.ValidObject();
            //if (!validResult)
            //{
            //    return OperateResult.CreateFailResult();
            //}
            return OperateResult.CreateSuccessResult();
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
    }
}
