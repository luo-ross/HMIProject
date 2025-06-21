using Microsoft.Extensions.DependencyInjection;
using RS.Annotation.Enums;
using RS.Annotation.IBLL;
using RS.Commons;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
namespace RS.Annotation.Views
{
    /// <summary>
    /// RegisterView.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterView : RSDialog
    {
        public RegisterViewModel ViewModel { get; set; }
        private readonly IGeneralBLL RegisterBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        public RegisterView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as RegisterViewModel;
            this.RegisterBLL = App.ServiceProvider?.GetService<IGeneralBLL>();
            this.CryptographyBLL = App.ServiceProvider?.GetService<ICryptographyBLL>();
        }


        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            bool isReisterSuccess = false;
            var result = await this.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
              {
                  OperateResult? operateResult = null;
                  switch (this.ViewModel.TaskStatus)
                  {
                      case RegisterTaskStatus.GetEmailVerify:
                          //this.ParentWin.SetLoadingText("正在获取邮箱验证码...");
                          operateResult = await GetEmailVerifyAsync();
                          break;
                      case RegisterTaskStatus.EmailVerifyValid:
                          //this.ParentWin.SetLoadingText("正在校验验证码...");
                          operateResult = await EmailVerifyValidAsync();
                          if (operateResult.IsSuccess)
                          {
                              isReisterSuccess = true;
                          }
                          break;
                      case RegisterTaskStatus.GetSMSVerify:
                          //this.ParentWin.SetLoadingText("正在获取短信验证码...");
                          operateResult = await GetSMSVerifyAsync();
                          break;
                      case RegisterTaskStatus.SMSVerifyValid:
                          //this.ParentWin.SetLoadingText("正在校验验证码...");
                          operateResult = await SMSVerifyValidAsync();
                          break;
                  }
                  return operateResult;
              });

            if (result.IsSuccess && isReisterSuccess)
            {
                //注册成功提示
                await this.Dispatcher.Invoke(async () =>
                {
                    await this.GetMessageBox().ShowMessageAsync("注册成功");
                });
            }




        }

        /// <summary>
        /// 获取邮箱验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult> GetEmailVerifyAsync()
        {
            //注册信息验证
            var emailRegisterModelValidPropertyResult = EmailRegisterModelValidProperty();
            if (!emailRegisterModelValidPropertyResult.IsSuccess)
            {
                return emailRegisterModelValidPropertyResult;
            }

            //如果邮箱验证通过 则往服务端发起验证码发送请求，
            var getVerifyModelResult = await this.GetVerifyModelAsync();
            if (!getVerifyModelResult.IsSuccess)
            {
                return getVerifyModelResult;
            }
            this.Dispatcher.Invoke(() =>
            {
                this.TxtEmailVerify.VerifyResultModel = getVerifyModelResult.Data;
            });

            //如果验证码发送成功 返回成功
            this.ViewModel.TaskStatus = RegisterTaskStatus.EmailVerifyValid;

            return OperateResult.CreateSuccessResult();
        }

        private async Task<OperateResult<VerifyModel>> GetVerifyModelAsync()
        {

            //如果邮箱验证通过 则往服务端发起验证码发送请求，
            var getEmailVerifyResult = await RSAppAPI.Register.GetEmailVerify.AESHttpPostAsync<EmailRegisterPostModel, RegisterVerifyModel>(new EmailRegisterPostModel()
            {
                Email = this.ViewModel.EmailRegisterModel.Email,
                //这里需要将秘密进行SHA256加密
                Password = this.CryptographyBLL.GetSHA256HashCode(this.ViewModel.EmailRegisterModel.Password),
            }, nameof(RSAppAPI));
            if (!getEmailVerifyResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<VerifyModel>(getEmailVerifyResult);
            }
            this.ViewModel.RegisterVerifyModel = getEmailVerifyResult.Data;


            return OperateResult.CreateSuccessResult(new VerifyModel()
            {
                IsSuccess = getEmailVerifyResult.IsSuccess,
                ExpireTime = getEmailVerifyResult.Data.ExpireTime,
            });
        }

        /// <summary>
        /// 校验邮箱验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> EmailVerifyValidAsync()
        {
            //验证用户是否输入验证码
            var validResult = this.ViewModel.EmailRegisterModel.ValidProperty(this.ViewModel.EmailRegisterModel.Verify, nameof(this.ViewModel.EmailRegisterModel.Verify));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("邮箱验证码输入验证失败");
            }

            //第一步 在本地先验证验证码是否失效
            var expireTime = this.ViewModel.RegisterVerifyModel.ExpireTime;
            if (expireTime <= DateTime.Now.ToTimeStamp())
            {
                this.ViewModel.TaskStatus = RegisterTaskStatus.GetEmailVerify;
                return OperateResult.CreateFailResult("邮箱验证码失效");
            }

            //往服务端发起请求验证用户输入的验证码是否和服务端存储的验证码一致
            var emailVerifyValidResult = await RSAppAPI.Register.EmailVerifyValid.AESHttpPostAsync(new RegisterVerifyValidModel()
            {
                RegisterSessionId = this.ViewModel.RegisterVerifyModel.RegisterSessionId,
                Verify = this.ViewModel.EmailRegisterModel.Verify,
            }, nameof(RSAppAPI));
            if (!emailVerifyValidResult.IsSuccess)
            {
                return emailVerifyValidResult;
            }



            await this.Dispatcher.BeginInvoke(() =>
            {
                //如果成功
                this.RegisterEnd?.Invoke(false);
            });

            //this.ViewModel.TaskStatus = RegisterTaskStatus.GetSMSVerify;



            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> GetSMSVerifyAsync()
        {
            //验证用户手机号码是否输出正确
            var validResult = this.ViewModel.SMSRegisterModel.ValidProperty(this.ViewModel.SMSRegisterModel.Phone, nameof(this.ViewModel.SMSRegisterModel.Phone));
            if (!validResult)
            {
                return OperateResult.CreateFailResult("手机号输入验证失败");
            }
            //往服务发起发送手机短信验证码的请求



            //如果成功
            this.ViewModel.TaskStatus = RegisterTaskStatus.SMSVerifyValid;

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult?> SMSVerifyValidAsync()
        {
            //验证用户是否输入手机短信验证码
            var validResult = this.ViewModel.SMSRegisterModel.ValidProperty(this.ViewModel.SMSRegisterModel.Verify, nameof(this.ViewModel.SMSRegisterModel.Verify));
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
        private async Task<VerifyModel> TxtEmailVerify_GetVerifyClick()
        {
            VerifyModel verifyModel = new VerifyModel();
            var parentWin = this.TryFindParent<RSWindow>();
            var loadingInvokeResult = await parentWin.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
               {
                   //如果邮箱验证通过 则往服务端发起验证码发送请求，
                   return await this.GetVerifyModelAsync();
               });
            if (loadingInvokeResult.IsSuccess)
            {
                return verifyModel;
            }

            return null;
        }

        /// <summary>
        /// 再次获取短信验证码
        /// </summary>
        /// <returns></returns>
        private async Task<VerifyModel> TxtSMSVerify_GetVerifyClick()
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
