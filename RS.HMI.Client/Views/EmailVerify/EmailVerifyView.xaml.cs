using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Runtime;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
    /// EmailVerifyView.xaml 的交互逻辑
    /// </summary>
    public partial class EmailVerifyView : RSUserControl
    {
        private readonly EmailVerifyViewModel ViewModel;
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private List<TextBox> VerifyCodeInputList { get; set; }
        public EmailVerifyView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as EmailVerifyViewModel;
            if (App.AppHost != null)
            {
                this.GeneralBLL = App.AppHost.Services.GetRequiredService<IGeneralBLL>();
                this.CryptographyBLL = App.AppHost.Services.GetRequiredService<ICryptographyBLL>();
            }
            VerifyCodeInputList = new List<TextBox>();
            VerifyCodeInputList.Add(this.TxtVerifyCode0);
            VerifyCodeInputList.Add(this.TxtVerifyCode1);
            VerifyCodeInputList.Add(this.TxtVerifyCode2);
            VerifyCodeInputList.Add(this.TxtVerifyCode3);
            VerifyCodeInputList.Add(this.TxtVerifyCode4);
            VerifyCodeInputList.Add(this.TxtVerifyCode5);
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

            //如果验证成功
            if (!operateResult.IsSuccess)
            {
                this.TryFindParent<RSWindow>()?.ShowInfoAsync(operateResult.Message, InfoType.Warning);
                return;
            }
        }



        /// <summary>
        /// 验证码确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerifyConfirm_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
