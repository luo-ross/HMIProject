using Microsoft.Extensions.DependencyInjection;
using RS.WPFApp.Enums;
using RS.WPFApp.IBLL;
using RS.Commons;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
namespace RS.WPFApp.Views
{
    /// <summary>
    /// RegisterView.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterView : RSUserControl
    {
        public RegisterViewModel ViewModel { get; set; }
        private readonly IGeneralService RegisterService;
        private readonly ICryptographyService CryptographyService;
        public RegisterView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as RegisterViewModel;
            this.RegisterService = App.AppHost?.Services.GetService<IGeneralService>();
            this.CryptographyService = App.AppHost?.Services.GetService<ICryptographyService>();
        }


        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            bool isReisterSuccess = false;
            var result = await this.InvokeLoadingActionAsync(async () =>
              {
                  OperateResult? operateResult = null;
                  switch (this.ViewModel.TaskStatus)
                  {
                      case RegisterTaskStatus.GetEmailVerification:
                          //this.ParentWin.SetLoadingText("正在获取邮箱验证码...");
                          operateResult = await GetEmailVerificationAsync();
                          break;
                      case RegisterTaskStatus.EmailVerificationValid:
                          //this.ParentWin.SetLoadingText("正在校验验证码...");
                          operateResult = await EmailVerificationValidAsync();
                          if (operateResult.IsSuccess)
                          {
                              isReisterSuccess = true;
                          }
                          break;
                      case RegisterTaskStatus.GetSMSVerification:
                          //this.ParentWin.SetLoadingText("正在获取短信验证码...");
                          operateResult = await GetSMSVerificationAsync();
                          break;
                      case RegisterTaskStatus.SMSVerificationValid:
                          //this.ParentWin.SetLoadingText("正在校验验证码...");
                          operateResult = await SMSVerificationValidAsync();
                          break;
                  }
                  return operateResult;
              });

            if (result.IsSuccess && isReisterSuccess)
            {
                //注册成功提示
                await this.Dispatcher.Invoke(async () =>
                {
                    await this.MessageBox.ShowAsync("注册成功");
                });
            }




        }

        /// <summary>
        /// 获取邮箱验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult> GetEmailVerificationAsync()
        {
            //注册信息验证
            var emailRegisterModelValidPropertyResult = EmailRegisterModelValidProperty();
            if (!emailRegisterModelValidPropertyResult.IsSuccess)
            {
                return emailRegisterModelValidPropertyResult;
            }

            //如果邮箱验证通过 则往服务端发起验证码发送请求，
            var getVerificationModelResult = await this.GetVerificationModelAsync();
            if (!getVerificationModelResult.IsSuccess)
            {
                return getVerificationModelResult;
            }
            this.Dispatcher.Invoke(() =>
            {
                this.TxtEmailVerification.VerificationResultModel = getVerificationModelResult.Data;
            });

            //如果验证码发送成功 返回成功
            this.ViewModel.TaskStatus = RegisterTaskStatus.EmailVerificationValid;

            return OperateResult.CreateSuccessResult();
        }

        private async Task<OperateResult<VerificationModel>> GetVerificationModelAsync()
        {

            //如果邮箱验证通过 则往服务端发起验证码发送请求，
            var getEmailVerificationResult = await RSAppAPI.Register.GetEmailVerification.AESHttpPostAsync<EmailRegisterPostModel, RegisterVerificationModel>(nameof(RSAppAPI), new EmailRegisterPostModel()
            {
                Email = this.ViewModel.EmailRegisterModel.Email,
                //这里需要将秘密进行SHA256加密
                Password = this.CryptographyService.GetSHA256HashCode(this.ViewModel.EmailRegisterModel.Password),
            });
            if (!getEmailVerificationResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<VerificationModel>(getEmailVerificationResult);
            }
            this.ViewModel.RegisterVerificationModel = getEmailVerificationResult.Data;


            return OperateResult.CreateSuccessResult(new VerificationModel()
            {
                IsSuccess = getEmailVerificationResult.IsSuccess,
                ExpireTime = getEmailVerificationResult.Data.ExpireTime,
            });
        }

        /// <summary>
        /// 校验邮箱验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> EmailVerificationValidAsync()
        {
            //验证用户是否输入验证码
            var validResult = this.ViewModel.EmailRegisterModel.ValidProperty(this.ViewModel.EmailRegisterModel.Verification, nameof(this.ViewModel.EmailRegisterModel.Verification));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("邮箱验证码输入验证失败");
            }

            //第一步 在本地先验证验证码是否失效
            var expireTime = this.ViewModel.RegisterVerificationModel.ExpireTime;
            if (expireTime <= DateTime.Now.ToTimeStamp())
            {
                this.ViewModel.TaskStatus = RegisterTaskStatus.GetEmailVerification;
                return OperateResult.CreateFailResult("邮箱验证码失效");
            }

            //往服务端发起请求验证用户输入的验证码是否和服务端存储的验证码一致
            var emailVerificationValidResult = await RSAppAPI.Register.EmailVerificationValid.AESHttpPostAsync(nameof(RSAppAPI), new RegisterVerificationValidModel()
            {
                Token = this.ViewModel.RegisterVerificationModel.Token,
                Verification = this.ViewModel.EmailRegisterModel.Verification,
            });
            if (!emailVerificationValidResult.IsSuccess)
            {
                return emailVerificationValidResult;
            }



            await this.Dispatcher.BeginInvoke(() =>
            {
                //如果成功
                this.RegisterEnd?.Invoke(false);
            });

            //this.ViewModel.TaskStatus = RegisterTaskStatus.GetSMSVerification;



            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> GetSMSVerificationAsync()
        {
            //验证用户手机号码是否输出正确
            var validResult = this.ViewModel.SMSRegisterModel.ValidProperty(this.ViewModel.SMSRegisterModel.Phone, nameof(this.ViewModel.SMSRegisterModel.Phone));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("手机号输入验证失败");
            }
            //往服务发起发送手机短信验证码的请求



            //如果成功
            this.ViewModel.TaskStatus = RegisterTaskStatus.SMSVerificationValid;

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> SMSVerificationValidAsync()
        {
            //验证用户是否输入手机短信验证码
            var validResult = this.ViewModel.SMSRegisterModel.ValidProperty(this.ViewModel.SMSRegisterModel.Verification, nameof(this.ViewModel.SMSRegisterModel.Verification));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("短信验证码输入验证失败");
            }
            //往服务发起发送手机短信验证请求



            //如果成功 就进入后面的业务逻辑
            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 邮箱注册数据输入验证
        /// </summary>
        /// <returns></returns>
        private OperateResult EmailRegisterModelValidProperty()
        {
            //验证用户输入邮箱是否正确
            var validResult = this.ViewModel.EmailRegisterModel.ValidProperty(this.ViewModel.EmailRegisterModel.Email, nameof(this.ViewModel.EmailRegisterModel.Email));

            if (!validResult)
            {
                return OperateResult.CreateFailResult("邮箱输入验证失败");
            }

            //验证用户输入密码是否正确
            validResult = this.ViewModel.EmailRegisterModel.ValidProperty(this.ViewModel.EmailRegisterModel.Password, nameof(this.ViewModel.EmailRegisterModel.Password));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("密码输入验证失败");
            }

            //验证用户输入确认密码是否正确
            validResult = this.ViewModel.EmailRegisterModel.ValidProperty(this.ViewModel.EmailRegisterModel.PasswordConfirm, nameof(this.ViewModel.EmailRegisterModel.PasswordConfirm));

            if (!validResult)
            {
                return OperateResult.CreateFailResult("确认密码输入验证失败");
            }

            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 再次获取邮箱验证码
        /// </summary>
        /// <returns></returns>
        private async Task<VerificationModel> TxtEmailVerification_GetVerificationClick()
        {
            VerificationModel verificationModel = new VerificationModel();
            var parentWin = this.TryFindParent<RSWindow>();
            var loadingInvokeResult = await parentWin.InvokeLoadingActionAsync(async () =>
               {
                   //如果邮箱验证通过 则往服务端发起验证码发送请求，
                   return await this.GetVerificationModelAsync();
               });
            if (loadingInvokeResult.IsSuccess)
            {
                return verificationModel;
            }

            return null;
        }

        /// <summary>
        /// 再次获取短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<VerificationModel> TxtSMSVerification_GetVerificationClick()
        {

            return null;
        }

        /// <summary>
        /// 注册结束 注册成功传递true 否则false
        /// </summary>
        public event Action<bool> RegisterEnd;

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            RegisterEnd?.Invoke(false);
        }


    }
}
