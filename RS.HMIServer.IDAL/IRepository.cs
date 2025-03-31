using RS.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IDAL
{
    public interface IRepository
    {
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="entity">数据库实体</param>
        /// <returns></returns>
        Task<OperateResult> InsertAsync<TEntity>(TEntity entity) where TEntity : class;


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="entity">删除实体</param>
        /// <returns></returns>
        Task<OperateResult> DeleteAsync<TEntity>(TEntity entity) where TEntity : class;


        /// <summary>
        /// 更改数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="entity">更新实体</param>
        /// <returns></returns>
        Task<OperateResult> UpdateAsync<TEntity>(TEntity entity) where TEntity : class;


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类型</typeparam>
        /// <param name="keyValue">数据库主键</param>
        /// <returns></returns>
        Task<OperateResult<TEntity>> FindAsync<TEntity>(params object?[]? keyValues) where TEntity : class;

        /// <summary>
        ///  返回第一个数据 如果没有返回null
        /// </summary>
        /// <typeparam name="TEntity">查询实体类型</typeparam>
        /// <param name="predicate">Lamda查询条件</param>
        /// <returns></returns>
        Task<OperateResult<TEntity>> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;


        /// <summary>
        ///  返回第一个数据 如果没有返回null
        /// </summary>
        /// <typeparam name="TEntity">查询实体类型</typeparam>
        /// <returns></returns>
        Task<OperateResult<TEntity>> FirstOrDefaultAsync<TEntity>() where TEntity : class;


        /// <summary>
        /// 查询是否存在任何数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Task<OperateResult> Any<TEntity>() where TEntity : class;


        /// <summary>
        /// 根据条件查询是否存在任何数据
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        Task<OperateResult> Any<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

    }
}
