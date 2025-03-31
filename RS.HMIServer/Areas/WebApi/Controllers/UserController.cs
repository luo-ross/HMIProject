
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using RS.HMIServer.IBLL;


namespace RS.HMIServer.Areas.WebApi.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class UserController : BaseController
    {
        /// <summary>
        /// 用户服务接口
        /// </summary>
        private readonly IUserService UserService;

        private readonly ILogService LogService;
        public UserController(IUserService userService, ILogService logService)
        {
            UserService = userService;
            LogService = logService;
        }

        /// <summary>
        /// 获取用户接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [FilterConfig]
        public async Task<OperateResult<AESEncryptModel>> GetUser()
        {
            return await UserService.GetUsersAsync(SessionId);
        }

        /// <summary>
        /// 验证登录
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<OperateResult> ValidLogin(AESEncryptModel aesEncryptModel)
        {
            return await UserService.ValidLoginAsync(aesEncryptModel, SessionId);
        }

    }
}
