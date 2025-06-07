using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.Entity;
using RS.HMIServer.IDAL;
using RS.Models;
using StackExchange.Redis;
using CommunityToolkit.Common;
using MathNet.Numerics.Statistics.Mcmc;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using RS.Commons.Enums;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
namespace RS.HMIServer.DAL
{

    /// <summary>
    /// 用户数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(IRoleDAL), ServiceLifetime.Transient)]
    internal class RoleDAL : Repository, IRoleDAL
    {

        public RoleDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext)
        {
            this.RSAppDb = rsAppDb;
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="roleModel">角色数据</param>
        /// <returns></returns>
        public async Task<OperateResult<RoleModel>> AddRoleAsync(RoleModel roleModel)
        {
            //数据预处理
            var operateResult = await DataPreprocessingAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            roleModel = operateResult.Data;

            //创建新的角色实体
            RoleEntity roleEntity = new RoleEntity()
            {
                CompanyId = roleModel.CompanyId,
                Description = roleModel.Description,
                Name = roleModel.Name,
            };

            var insertResult = await this.InsertAsync(roleEntity);
            if (!insertResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<RoleModel>(insertResult);
            }
            //获取到主键值
            roleModel.Id = roleEntity.Id;
            return OperateResult.CreateSuccessResult(roleModel);
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult<PageDataModel<RoleModel>>> GetRolesAsync(Pagination pagination)
        {

            OperateResult<PageDataModel<RoleModel>> getPaginationListReuslt =
                await this.GetPaginationListWithCreateUpateDeleteByAsync<RoleEntity, RoleModel>(pagination,
               async (paginationList) =>
                {
                    //分页完后获取公司
                    var companyIdList = paginationList.Select(t => t.CompanyId).ToList();
                    var companyList = await this.RSAppDb.Company
                        .Where(t => companyIdList.Contains(t.Id))
                        .Select(t => new { t.Name, t.Id })
                        .ToListAsync();

                    //使用Join 获取公司名称
                    var dataList = paginationList.GroupJoin(companyList,
                           a => a.CompanyId,
                           b => b.Id,
                           (a, b) =>
                           {
                               a.CompanyName = b.FirstOrDefault()?.Name;
                               return a;
                           }).ToList();

                    return dataList;
                });
            if (!getPaginationListReuslt.IsSuccess)
            {
                return getPaginationListReuslt;
            }

            //这里有可能还会有别的逻辑

            return getPaginationListReuslt;
        }


        /// <summary>
        /// 更新角色名称
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateNameAsync(RoleModel roleModel)
        {

            if (roleModel.Id == null)
            {
                return OperateResult.CreateFailResult("角色主键不能为空");
            }

            roleModel.Name = roleModel.Name?.FixHtml();

            if (string.IsNullOrEmpty(roleModel.Name)
                || string.IsNullOrWhiteSpace(roleModel.Name))
            {
                return OperateResult.CreateFailResult("角色名称不能为空");
            }

            var effectRows = await this.RSAppDb.Role
                  .Where(t => t.Id == roleModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.Name, roleModel.Name));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 更新角色备注
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateDescriptionAsync(RoleModel roleModel)
        {
            if (roleModel.Id == null)
            {
                return OperateResult.CreateFailResult("角色主键不能为空");
            }

            roleModel.Description = roleModel.Description?.FixHtml();

            var effectRows = await this.RSAppDb.Role
               .Where(t => t.Id == roleModel.Id)
               .ExecuteUpdateAsync(setters =>
               setters.SetProperty(b => b.Description, roleModel.Description));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 更新角色公司
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateCompanyIdAsync(RoleModel roleModel)
        {
            if (roleModel.Id == null)
            {
                return OperateResult.CreateFailResult("角色主键不能为空");
            }

            var effectRows = await this.RSAppDb.Role
                  .Where(t => t.Id == roleModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.CompanyId, roleModel.CompanyId));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleModel">角色参数</param>
        /// <returns></returns>
        public async Task<OperateResult<RoleModel>> DeleteRoleAsync(RoleModel roleModel)
        {
            //删除角色是一个比较复杂的逻辑过程 这里简单实现
            if (roleModel.Id == null)
            {
                return OperateResult.CreateFailResult<RoleModel>("角色主键不能为空");
            }

            roleModel.IsDelete = true;
            roleModel.DeleteTime = DateTime.Now;

            //这里待处理
            //roleModel.DeleteId = null;
            //roleModel.DeleteBy = null;

            var effectRows = await this.RSAppDb.Role
                  .Where(t => t.Id == roleModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.IsDelete, roleModel.IsDelete)
                  .SetProperty(b => b.DeleteTime, roleModel.DeleteTime)
                  .SetProperty(b => b.DeleteId, roleModel.DeleteId)
                  );

            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult<RoleModel>("更新失败");
            }

            return OperateResult.CreateSuccessResult(roleModel);
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="roleModel">更新角色</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateRoleAsync(RoleModel roleModel)
        {
            
            //RoleEntity roleEntity = new RoleEntity()
            //{
            //    CompanyId = roleModel.CompanyId,
            //    CreateId = roleModel.CreateId,
            //    CreateTime = DateTime.Now,
            //    DeleteId = roleModel.DeleteId,
            //    DeleteTime = roleModel.DeleteTime,
            //    Description = roleModel.Description,
            //    IsDelete = roleModel.IsDelete,
            //    Name = roleModel.Name,
            //    UpdateId = roleModel.UpdateId,
            //    UpdateTime = roleModel.UpdateTime,
            //    Id=roleModel.Id.ToLong(),
            //};

            //数据预处理
            var operateResult = await DataPreprocessingAsync(roleModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            roleModel = operateResult.Data;

            if (roleModel.Id == null)
            {
                return OperateResult.CreateFailResult<RoleModel>("角色主键不能为空");
            }

            var setPropertiesExpression = this.CreateSetPropertiesExpression<RoleModel,RoleEntity>(roleModel);

            var effectRows = await this.RSAppDb.Role
                  .Where(t => t.Id == roleModel.Id)
                  .ExecuteUpdateAsync(setPropertiesExpression);
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult<RoleModel>("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 数据预处理
        /// </summary>
        /// <param name="roleModel"></param>
        /// <returns></returns>
        private async Task<OperateResult<RoleModel>> DataPreprocessingAsync(RoleModel roleModel)
        {
            //验证公司
            if (roleModel.CompanyId != null)
            {
                var companyEntity = await this.RSAppDb.Company.FirstOrDefaultAsync(t => t.Id == roleModel.CompanyId);
                if (companyEntity == null)
                {
                    return OperateResult.CreateFailResult<RoleModel>("绑定公司不存在！");
                }
            }
            roleModel.Name = roleModel.Name?.FixHtml();
            roleModel.Description = roleModel.Description?.FixHtml();

            if (string.IsNullOrEmpty(roleModel.Name)
                || string.IsNullOrWhiteSpace(roleModel.Name))
            {
                return OperateResult.CreateFailResult<RoleModel>("角色名称不能为空");
            }
            return OperateResult.CreateSuccessResult<RoleModel>(roleModel);
        }



    }
}
