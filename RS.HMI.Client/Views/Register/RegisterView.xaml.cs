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
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class RegisterView : RSUserControl
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly RegisterViewModel ViewModel;
        private RegisterVerifyModel RegisterVerifyModel;
        private DispatcherTimer DispatcherTimer;
        private List<TextBox> VerifyCodeInputList;

        #region 自定义事件
        public event Action OnBtnReturnClick;
        #endregion


        public RegisterView(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL)
        {
            InitializeComponent();
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.ViewModel = this.DataContext as RegisterViewModel;

            VerifyCodeInputList = new List<TextBox>();
            VerifyCodeInputList.Add(this.TxtVerifyCode0);
            VerifyCodeInputList.Add(this.TxtVerifyCode1);
            VerifyCodeInputList.Add(this.TxtVerifyCode2);
            VerifyCodeInputList.Add(this.TxtVerifyCode3);
            VerifyCodeInputList.Add(this.TxtVerifyCode4);
            VerifyCodeInputList.Add(this.TxtVerifyCode5);
        }

  

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


            var operateResult = await this.InvokeLoadingActionAsync(async () =>
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

           this. RegisterVerifyModel = operateResult.Data;

            var expireTime = this.RegisterVerifyModel.ExpireTime.FromUnixTimeStamp();

            TimeSpan remainingTime = expireTime - DateTime.Now;
            this.ViewModel.RemainingTime = remainingTime.TotalSeconds;
            this.DispatcherTimer = new DispatcherTimer();
            this.DispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            this.DispatcherTimer.Tick += (s, e) =>
            {
                remainingTime = expireTime - DateTime.Now;
                if (remainingTime.TotalSeconds > 0)
                {
                    this.ViewModel.RemainingTime = remainingTime.TotalSeconds;
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.PART_EmailVerify.Visibility = Visibility.Collapsed;
                    });
                    this.DispatcherTimer.Stop();
                }
            };
            this.DispatcherTimer.Start();
            this.PART_EmailVerify.Visibility = Visibility.Visible;
        }

        #region 邮箱验证


        [Description("邮箱绑定")]
        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(RegisterView), new PropertyMetadata(null));



        /// <summary>
        /// 验证码确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnVerifyConfirm_Click(object sender, RoutedEventArgs e)
        {
            var textList = VerifyCodeInputList.Select(t => t.Text).ToList();
            var verify = string.Join("", textList);

            if (verify.Length!=6)
            {
                this.TryFindParent<RSWindow>()?.ShowInfoAsync("验证码验证失败！", InfoType.Warning);
                return;
            }

            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.ProgressBar,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true,
            };

            var operateResult = await this.InvokeLoadingActionAsync(async () =>
            {

                var registerVerifyValidModel = new RegisterVerifyValidModel();
                registerVerifyValidModel.RegisterSessionId = this.RegisterVerifyModel.RegisterSessionId;
                registerVerifyValidModel.Verify = verify;
               
               
                //获取邮箱验证码结果
                var emailVerifyValidResult = await RSAppAPI.Register.EmailVerifyValid.AESHttpPostAsync<RegisterVerifyValidModel, OperateResult>(nameof(RSAppAPI), registerVerifyValidModel);

                if (!emailVerifyValidResult.IsSuccess)
                {
                    return emailVerifyValidResult;
                }

                return emailVerifyValidResult;
            }, loadingConfig);

            //如果失败
            if (!operateResult.IsSuccess)
            {
                this.TryFindParent<RSWindow>()?.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }
        }

        private void TxtVerifyCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var index = this.VerifyCodeInputList.IndexOf(textBox);
            if (!string.IsNullOrEmpty(textBox.Text) && index < 5)
            {
                this.VerifyCodeInputList[index + 1].Focus();
            }
        }

        private void TxtVerifyCode_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var index = this.VerifyCodeInputList.IndexOf(textBox);
            if (Keyboard.IsKeyDown(Key.Back))
            {
                textBox.Text = null;
                if (index > 0)
                {
                    this.VerifyCodeInputList[index - 1].Focus();
                }
            }
            else if (
                (Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl))
                && Keyboard.IsKeyDown(Key.V)
                )
            {
                IDataObject data = Clipboard.GetDataObject();
                if (data != null && data.GetDataPresent(DataFormats.Text))
                {
                    string clipboardText = data.GetData(DataFormats.Text)?.ToString();
                    string replacedText = clipboardText.Replace(Environment.NewLine, "").Replace(" ", "");
                    var verifyCodeList = replacedText.Take(6).ToList();
                    for (int i = 0; i < verifyCodeList.Count; i++)
                    {
                        var code = verifyCodeList[i].ToString();
                        var textInput = this.VerifyCodeInputList[i];
                        textInput.Text = code;
                        textInput.Focus();
                        textInput.CaretIndex = 1;
                    }
                }
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            //停止定时器
            if (this.DispatcherTimer != null)
            {
                this.DispatcherTimer.Stop();
                this.DispatcherTimer = null;
            }

            //清除验证码信息
            foreach (var item in VerifyCodeInputList)
            {
                item.Text = null;
            }

            
            if (this.PART_EmailVerify.Visibility == Visibility.Visible)
            {
                this.PART_EmailVerify.Visibility = Visibility.Collapsed;
                return;
            }

            if (this.PART_Register.Visibility == Visibility.Collapsed)
            {
                this.PART_Register.Visibility = Visibility.Visible;
            }

            this.Visibility = Visibility.Collapsed;
            this.OnBtnReturnClick?.Invoke();
        }
        #endregion

    }


}
