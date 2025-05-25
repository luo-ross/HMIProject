using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;

namespace RS.HMIServer.BLL
{
    [ServiceInjectConfig(typeof(IRoleBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class RoleBLL : IRoleBLL
    {
        /// <summary>
        /// 角色数据仓储接口
        /// </summary>
        private readonly IRoleDAL RoleDAL;

        /// <summary>
        /// 通用服务接口
        /// </summary>
        private readonly IGeneralBLL GeneralBLL;

        /// <summary>
        /// 加解密服务接口
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;
        public RoleBLL(IRoleDAL roleDAL, IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL)
        {
            this.RoleDAL = roleDAL;
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> AddRoleAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RoleModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var roleModel = getAESDecryptResult.Data;

            //将数据写入到数据库
            var operateResult = await this.RoleDAL.AddRoleAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //返回数据主键
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(operateResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }
            return getAESEncryptResult;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetRolesAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<Pagination>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var pagination = getAESDecryptResult.Data;



            //这是获取角色数据
            var getRolessResult = await this.RoleDAL.GetRolesAsync(pagination);
            if (!getRolessResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getRolessResult);
            }

            //获取加密数据
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(getRolessResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }
            return getAESEncryptResult;
        }

       

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateNameAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RoleModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var roleModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult operateResult = await this.RoleDAL.UpdateNameAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新绑定公司
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateCompanyIdAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RoleModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var roleModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult operateResult = await this.RoleDAL.UpdateCompanyIdAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新角色备注
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateDescriptionAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RoleModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var roleModel = getAESDecryptResult.Data;

            //将数据更新写入到数据库
            OperateResult operateResult = await this.RoleDAL.UpdateDescriptionAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> DeleteRoleAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RoleModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var roleModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult<RoleModel> operateResult = await this.RoleDAL.DeleteRoleAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //获取加密数据
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(operateResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }
    }
}
