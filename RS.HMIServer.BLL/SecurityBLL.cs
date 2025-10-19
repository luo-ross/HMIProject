using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Helper;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Models;
using RTools_NTS.Util;
using System.Collections.Generic;
using System.Security.Claims;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 安全服务
    /// </summary>
    [ServiceInjectConfig(typeof(ISecurityBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class SecurityBLL : ISecurityBLL
    {

        /// <summary>
        /// 邮箱服务
        /// </summary>
        private readonly IEmailBLL EmailBLL;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;

        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly ISecurityDAL SecurityDAL;

        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly IGeneralDAL GeneralDAL;

        /// <summary>
        /// 通用服务
        /// </summary>
        private readonly IGeneralBLL GeneralBLL;

        /// <summary>
        /// 配置接口
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// 配置接口
        /// </summary>
        private readonly IOpenCVBLL OpenCVBLL;

        public SecurityBLL(
            ISecurityDAL securityDAL,
            IGeneralDAL generalDAL,
            IEmailBLL emailBLL,
            ICryptographyBLL cryptographyBLL,
            IGeneralBLL generalBLL,
            IConfiguration configuration,
            IOpenCVBLL openCVBLL
            )
        {
            this.SecurityDAL = securityDAL;
            this.GeneralDAL = generalDAL;
            this.EmailBLL = emailBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.GeneralBLL = generalBLL;
            this.Configuration = configuration;
            this.OpenCVBLL = openCVBLL;
        }


        public async Task<OperateResult> PasswordResetEmailSendAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<EmailSecurityModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var emailSecurityModel = getAESDecryptResult.Data;

            if (emailSecurityModel == null)
            {
                throw new ArgumentNullException(nameof(emailSecurityModel));
            }
            string webHost = this.Configuration["WebHost"];
            if (string.IsNullOrEmpty(webHost) || string.IsNullOrWhiteSpace(webHost))
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            if (string.IsNullOrEmpty(emailSecurityModel.Email) || string.IsNullOrWhiteSpace(emailSecurityModel.Email))
            {
                return OperateResult.CreateFailResult("邮件地址不能为空");
            }

            //验证邮箱格式是否正确
            if (!emailSecurityModel.Email.IsEmail())
            {
                return OperateResult.CreateFailResult("邮箱格式不正确！");
            }

            //验证用户是否存在
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == emailSecurityModel.Email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            //创建修改密码会话
            string passwordResetToken = Guid.NewGuid().ToString();  //创建会话主键
            var operateResult = await this.SecurityDAL.CreateEmailPasswordResetSessionAsync(passwordResetToken, new EmailSecurityModel()
            {
                Email = emailSecurityModel.Email,
            });

            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            emailSecurityModel.ResetLink = $"{webHost}/EmailPasswordReset?Email={Uri.EscapeDataString(emailSecurityModel.Email)}&Token={passwordResetToken}";

            operateResult = await this.EmailBLL.SendPassResetAsync(emailSecurityModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            //发送给用户后，等待用户点击链接进行密码修改

            return operateResult;

        }

        public async Task<OperateResult> EmailPasswordResetConfirmAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<EmailPasswordConfirmModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var emailPasswordConfirmModel = getAESDecryptResult.Data;

            //查看会话是否存在
            var validResult = await this.SecurityDAL.EmailPasswordResetSessionValidAsync(emailPasswordConfirmModel.Email, emailPasswordConfirmModel.Token);
            if (!validResult.IsSuccess)
            {
                return validResult;
            }
            var emailSecurityModel = validResult.Data;

            //判断用户是否相同
            if (!emailSecurityModel.Email.Equals(emailPasswordConfirmModel.Email))
            {
                return OperateResult.CreateFailResult("密码重置会话不存在");
            }
            //获取邮箱
            string email = emailPasswordConfirmModel.Email;
            //然后获取用户信息
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            return await this.SecurityDAL.EmailPasswordResetConfirmAsync(emailPasswordConfirmModel);
        }

        /// <summary>
        /// 获取验证码信息
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="audiences">客户端类型</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetImgVerifyModelAsync(string sessionId, string audiences)
        {
            //在这个创建验证码数据前简单验证一下用户是否有权限啥的
            OperateResult operateResult = await this.SecurityDAL.IsCanCreateImgVerifySessionAsync(sessionId);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //首先获取验证码图片数据
            var getVerifyImgInitModel = await this.OpenCVBLL.GetVerifyImgInitModelAsync();
            if (!getVerifyImgInitModel.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getVerifyImgInitModel);
            }
            var verifyImgInitModel = getVerifyImgInitModel.Data;

            //获取到图片信息后 将这些验证数据放在Redis缓存里
            OperateResult<string> createVerifySessionModelResult = await this.SecurityDAL.CreateVerifySessionModelAsync(verifyImgInitModel, sessionId);
            if (!createVerifySessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(createVerifySessionModelResult);
            }

            string verifySessionId = createVerifySessionModelResult.Data;

            //获取唯一Id
            verifyImgInitModel.VerifySessionId = verifySessionId;

            //这里还需要把验证码信息存起来，用于后面的用户进行验证使用
            ImgVerifyModel imgVerifyModel = new ImgVerifyModel()
            {
                ImgBtnPositionX = verifyImgInitModel.ImgBtnPositionX,
                ImgBtnPositionY = verifyImgInitModel.ImgBtnPositionY,
                IconHeight = verifyImgInitModel.IconHeight,
                IconWidth = verifyImgInitModel.IconWidth,
                ImgBuffer = verifyImgInitModel.ImgBuffer,
                ImgHeight = verifyImgInitModel.ImgHeight,
                ImgWidth = verifyImgInitModel.ImgWidth,
                VerifySessionId = verifyImgInitModel.VerifySessionId,
            };

            //AES对称加密
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(imgVerifyModel, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }



        /// <summary>
        /// 验证用户登录
        /// </summary>
        /// <param name="aesEncryptModel">AES对称加密</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> ValidLoginAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {

            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<LoginValidModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var loginValidModel = getAESDecryptResult.Data;

            //首先进行验证码验证
            OperateResult validVerifyResult = await this.ValidVerifyResultAsync(loginValidModel.VerifySessionId, loginValidModel.Verify);
            if (!validVerifyResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(validVerifyResult);
            }

            //查询用户信息
            var userEntityResult = await this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == loginValidModel.Email);
            if (!userEntityResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("用户名或者密码错误！");
            }
            var userEntity = userEntityResult.Data;

            //获取登录信息
            var logOnEntityResult = await this.SecurityDAL.FirstOrDefaultAsync<LogOnEntity>(t => t.UserId == userEntity.Id);
            if (!userEntityResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("用户名或者密码错误！");
            }
            var logOnEntity = logOnEntityResult.Data;

            //是否禁用
            if (!logOnEntity.IsEnable)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("当前用户已禁用！");
            }

            //通过用户输入的密码 结合注册时使用Salt 再生成一遍密码进行比对
            var newPassword = this.CryptographyBLL.GetSHA256HashCode($"{loginValidModel.Password}-{logOnEntity.Salt}");

            //比较密码是否相同
            if (!newPassword.Equals(logOnEntity.Password))
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("用户名或者密码错误！");
            }

            //如果验证成功还要获取角色相关信息
            string roleId = Guid.NewGuid().ToString();

            //创建新的会话
            string sessionIdNew = Guid.NewGuid().ToString();
            var claimList = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, sessionIdNew),
                new Claim(ClaimTypes.Role,roleId)
            };

            //通过JWT 生成Token  待处理
            var generateJWTTokenResult = this.GeneralBLL.GenerateJWTToken(claimList, audiences);
            if (!generateJWTTokenResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(generateJWTTokenResult);
            }
            string token = generateJWTTokenResult.Data;

            string appId = Guid.NewGuid().ToString();

            SessionModel sessionModel = new SessionModel();
            sessionModel.AppId = appId;
            sessionModel.AesKey = this.CryptographyBLL.GenerateAESKey();
            sessionModel.Token = token;


            //把创建好的会话数据写入到Redis进行存储
            string aesKeyProtect = CryptographyBLL.ProtectData(sessionModel.AesKey);
            string appIdProtect = CryptographyBLL.ProtectData(appId);
            var saveSessionModelResult = await GeneralDAL.SaveSessionModelAsync(new SessionModel()
            {
                AppId = appIdProtect,
                AesKey = aesKeyProtect,
                Token = token,
            }, sessionIdNew);

            if (!saveSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(saveSessionModelResult);
            }

            //获取默认图像
            if (string.IsNullOrEmpty(userEntity.UserPic))
            {
                var UserImgDefaultDir = Directory.GetCurrentDirectory();
                UserImgDefaultDir = Path.Combine(UserImgDefaultDir, "UserDefaultImg");
                var useImgDefaultList = Directory.GetFiles(UserImgDefaultDir);
                var userPic = useImgDefaultList[Random.Shared.Next(0,useImgDefaultList.Length)];
                userEntity.UserPic = userPic;
            }

            //返回加密数据给用户
            LoginResultModel loginResultModel = new LoginResultModel()
            {
                NickName = userEntity.NickName,
                UserImgUrl = File.ReadAllBytes(userEntity.UserPic),
                SessionModel = sessionModel,
            };

            //AES对称加密 这里加密的话还是得用旧的会话
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(loginResultModel, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            //加密完之后 需要把酒店SessionId会话删除
            var removeSessionModelResult = await this.GeneralDAL.RemoveSessionModelAsync(sessionId);
            if (!removeSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(removeSessionModelResult);
            }

            return getAESEncryptResult;
        }


        public async Task<OperateResult> ValidVerifyResultAsync(string verifySessionId, RectModel verify)
        {
            //获取验证码会话数据
            var getImgVerifySessionModelResult = await this.SecurityDAL.GetImgVerifySessionModelAsync(verifySessionId);
            if (!getImgVerifySessionModelResult.IsSuccess)
            {
                return getImgVerifySessionModelResult;
            }
            //获取到验证码会话内容
            var imgVerifySessionModel = getImgVerifySessionModelResult.Data;

            //获取可信度
            var credibility = this.CalculateIOU(imgVerifySessionModel.Rect, verify);

            //获取配置可信度
            if (!double.TryParse(this.Configuration["ImgVerifyCredibilityThreshold"], out double imgVerifyCredibilityThreshold))
            {
                imgVerifyCredibilityThreshold = 0.95;
            }
            if (credibility >= imgVerifyCredibilityThreshold)
            {
                return OperateResult.CreateSuccessResult();
            }
            else
            {
                return OperateResult.CreateFailResult("验证码验证失败");
            }
        }


        // 计算两个矩形框 IOU 的方法
        public double CalculateIOU(RectModel rect1, RectModel rect2)
        {
            // 参数校验
            if (rect1.Width <= 0 || rect1.Height <= 0 || rect2.Width <= 0 || rect2.Height <= 0)
            {
                return 0;
            }

            double xLeft = Math.Max(rect1.Left, rect2.Left);
            double yTop = Math.Max(rect1.Top, rect2.Top);
            double xRight = Math.Min(rect1.Left + rect1.Width, rect2.Left + rect2.Width);
            double yBottom = Math.Min(rect1.Top + rect1.Height, rect2.Top + rect2.Height);

            double intersectionArea = Math.Max(0, xRight - xLeft) * Math.Max(0, yBottom - yTop);

            double rect1Area = rect1.Width * rect1.Height;
            double rect2Area = rect2.Width * rect2.Height;
            double unionArea = rect1Area + rect2Area - intersectionArea;

            if (unionArea <= 0)
            {
                return 0; // 防止除零
            }
            return intersectionArea / unionArea;
        }

    }
}
