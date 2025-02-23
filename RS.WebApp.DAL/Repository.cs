using Microsoft.EntityFrameworkCore;
using RS.WebApp.Entity;
using RS.WebApp.DAL.SqlServer;
using RS.WebApp.IDAL;
using RS.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RS.WebApp.DAL
{
    /// <summary>
    /// 仓储基类
    /// </summary>
    internal class Repository : IRepository
    {
        /// <summary>
        /// 鉴权服务数据库上下文
        /// </summary>
        protected RSAppDbContext RSAppDb { get; set; }
        public Repository()
        {

        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="entity">数据库实体</param>
        /// <returns></returns>
        public async Task<OperateResult> InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                return OperateResult.CreateFailResult("实体不能为null");
            }

            //确保主键创建
            if (entity is BaseEntity baseEntity && baseEntity.Id == 0)
            {
                baseEntity.Create();
            }

            await this.RSAppDb.AddAsync(entity);
            var effectRow = await this.RSAppDb.SaveChangesAsync();
            if (effectRow == 0)
            {
                return OperateResult.CreateFailResult("新增数据失败");
            }
            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="entity">删除实体</param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            this.RSAppDb.Remove(entity);
            var effectRow = await this.RSAppDb.SaveChangesAsync();
            if (effectRow == 0)
            {
                return OperateResult.CreateFailResult("删除数据失败");
            }
            return OperateResult.CreateSuccessResult();

        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            this.RSAppDb.Update(entity);
            var effectRow = await this.RSAppDb.SaveChangesAsync();
            if (effectRow == 0)
            {
                return OperateResult.CreateFailResult("更改数据失败");
            }
            return OperateResult.CreateSuccessResult();
        }



        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="keyValue">数据库主键</param>
        /// <returns></returns>
        public async Task<OperateResult<TEntity>> FindAsync<TEntity>(params object?[]? keyValues) where TEntity : class
        {
            var entity = await this.RSAppDb.FindAsync<TEntity>(keyValues);
            if (entity == null)
            {
                return OperateResult.CreateFailResult<TEntity>("未查询到数据");
            }
            return OperateResult.CreateSuccessResult(entity);
        }


        /// <summary>
        ///  返回第一个数据 如果没有返回null
        /// </summary>
        /// <typeparam name="TEntity">查询实体类型</typeparam>
        /// <param name="predicate">Lamda查询条件</param>
        /// <returns></returns>
        public async Task<OperateResult<TEntity>> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entity = await this.RSAppDb.Set<TEntity>().FirstOrDefaultAsync(predicate);
            if (entity == null)
            {
                return OperateResult.CreateFailResult<TEntity>("未查询到数据");
            }
            return OperateResult.CreateSuccessResult(entity);
        }

        /// <summary>
        ///  返回第一个数据 如果没有返回null
        /// </summary>
        /// <typeparam name="TEntity">查询实体类型</typeparam>
        /// <returns></returns>
        public async Task<OperateResult<TEntity>> FirstOrDefaultAsync<TEntity>() where TEntity : class
        {
            var entity = await this.RSAppDb.Set<TEntity>().FirstOrDefaultAsync();
            if (entity == null)
            {
                return OperateResult.CreateFailResult<TEntity>("未查询到任何数据");
            }
            return OperateResult.CreateSuccessResult(entity);
        }

        /// <summary>
        /// 查询是否存在任何数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<OperateResult> Any<TEntity>() where TEntity : class
        {
            if (!this.RSAppDb.Set<TEntity>().Any())
            {
                return OperateResult.CreateFailResult("未查询到任何数据");
            }
            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 根据条件查询是否存在任何数据
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        public async Task<OperateResult> Any<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            if (!this.RSAppDb.Set<TEntity>().Any(predicate))
            {
                return OperateResult.CreateFailResult("未查询到任何数据");
            }
            return OperateResult.CreateSuccessResult();
        }

    }
}
