using RS.Commons;
using RS.Models;
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


        /// <summary>
        /// 获取创建人 更新人 和删除人信息
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        Task<OperateResult<List<T>>> GetCreateUpdateDeleteByAsync<T>(List<ModelBase> modelBaseList)
                 where T : ModelBase;


        /// <summary>
        /// 分页查询数据并且返回创建人 修改人 和删除人
        /// </summary>
        /// <typeparam name="TEntity">数据实体</typeparam>
        /// <typeparam name="TResult">返回实体</typeparam>
        /// <param name="pagination">分页实体</param>
        /// <param name="paginationCallBack">分页回调</param>
        /// <returns></returns>
        Task<OperateResult<PageDataModel<TResult>>> GetPaginationListWithCreateUpateDeleteByAsync<TEntity, TResult>(Pagination pagination,
           Func<List<TResult>, Task<List<TResult>>> paginationCallBack = null)
           where TEntity : class
           where TResult : ModelBase, new();


        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <typeparam name="TEntity">数据实体</typeparam>
        /// <typeparam name="TResult">返回实体</typeparam>
        /// <param name="pagination">分页实体</param>
        /// <param name="paginationCallBack">分页回调</param>
        /// <returns></returns>
        Task<OperateResult<PageDataModel<TResult>>> GetPaginationListAsync<TEntity, TResult>(Pagination pagination,
           Func<List<TResult>, Task<List<TResult>>> paginationCallBack = null)
           where TEntity : class
           where TResult : ModelBase, new();
    }
}
