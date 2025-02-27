using RS.Commons;
using RS.HMI.Client.Views.Home;
using RS.Widgets.Models;
using RS.Widgets.Controls;
using System.Diagnostics;
using System.Windows;
using RS.Widgets.Enums;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Models;
using RS.RESTfulApi;
using RS.Commons.Extensions;
using RS.HMI.IBLL;

namespace RS.HMI.Client.Views.Logoin
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class LoginView : RSWindow
    {
        /// <summary>
        /// 数据实体
        /// </summary>
        private readonly LoginViewModel ViewModel;
        private readonly IGeneralService GeneralService;
        private readonly ICryptographyService CryptographyService;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public LoginView(IGeneralService generalService, ICryptographyService cryptographyService)
        {
            InitializeComponent();
            this.GeneralService = generalService;
            this.CryptographyService = cryptographyService;
            this.ViewModel = this.DataContext as LoginViewModel;
            this.Closed += LoginView_Closed;
            this.Loaded += LoginView_Loaded;
        }

        private void LoginView_Closed(object? sender, EventArgs e)
        {
            
        }

        private async void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            var operateResult = await this.InvokeLoadingActionAsync(async () =>
            {

                var getSessionModelResult = await this.GeneralService.GetSessionModelAsync();
                if (!getSessionModelResult.IsSuccess)
                {
                    this.Dispatcher.Invoke(async () =>
                    {
                        await this.MessageBox.ShowAsync(getSessionModelResult.Message);
                    });
                }


                return OperateResult.CreateSuccessResult();
            }, new LoadingConfig()
            {
                LoadingType=LoadingType.RotatingAnimation,
            });

        }

        /// <summary>
        /// 超级连接跳转
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", e.Uri.ToString());
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 登录点击事件
        /// </summary>
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 验证用户名和密码数据
            var validResult = this.ViewModel.LoginModel.ValidObject();
            if (!validResult)
            {
                return;
            }

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

            var operateResult = await this.LoginForm.InvokeLoadingActionAsync(async () =>
              {

                  //模拟登录
                  await Task.Delay(2000);


                  ////this.SetLoadingText("正在登录中...");
                  ////验证用户登录
                  //var validLoginResult = await RSAppAPI.ValidLogin.AESHttpPostAsync(nameof(RSAppAPI), new LoginValidModel()
                  //{
                  //    UserName = this.ViewModel.PasswordLoginModel.UserName,
                  //    Password = this.CryptographyService.GetSHA256HashCode(this.ViewModel.PasswordLoginModel.Password),
                  //});


                  return OperateResult.CreateSuccessResult();
              }, loadingConfig);

            //如果验证成功
            if (operateResult.IsSuccess)
            {
                var homeView = App.AppHost?.Services.GetService<HomeView>();
                homeView?.Show();
                this.Close();
            }
        }





        /// <summary>
        /// 注册点击事件
        /// </summary>
        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            //注册信息验证
            var validResult = this.ViewModel.SignUpModel.ValidObject();
            if (!validResult)
            {
                return;
            }


        }


    }
}
