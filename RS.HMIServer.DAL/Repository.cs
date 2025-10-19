using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Org.BouncyCastle.Asn1.X509;
using RS.Commons;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.Entity;
using RS.HMIServer.IDAL;
using RS.Models;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RS.HMIServer.DAL
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
            if (entity is BaseEntity baseEntity && string.IsNullOrEmpty(baseEntity.Id))
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

        /// <summary>
        /// 获取创建人 更新人 和删除人信息
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public async Task<OperateResult<List<T>>> GetCreateUpdateDeleteByAsync<T>(List<ModelBase> modelBaseList)
            where T : ModelBase
        {
            //获取创建人
            var createIdList = modelBaseList.Select(t => t.CreateId).ToList();
            var createList = await this.RSAppDb.User
                .Where(t => createIdList.Contains(t.Id))
                .Select(t => new { t.NickName, t.Id })
                .ToListAsync();

            modelBaseList = modelBaseList.GroupJoin(createList,
               a => a.CreateId,
               b => b.Id,
               (a, b) =>
               {
                   a.CreateBy = b.FirstOrDefault()?.NickName;
                   return a;
               }).ToList();


            //获取更新人
            var updateIdList = modelBaseList.Select(t => t.UpdateId).ToList();
            var updateList = await this.RSAppDb.User
                .Where(t => updateIdList.Contains(t.Id))
                .Select(t => new { t.NickName, t.Id })
                .ToListAsync();

            modelBaseList = modelBaseList.GroupJoin(updateList,
               a => a.UpdateId,
               b => b.Id,
               (a, b) =>
               {
                   a.UpdateBy = b.FirstOrDefault()?.NickName;
                   return a;
               }).ToList();

            //获取删除人
            var deleteIdList = modelBaseList.Select(t => t.DeleteId).ToList();
            var deleteList = await this.RSAppDb.User
                .Where(t => deleteIdList.Contains(t.Id))
                .Select(t => new { t.NickName, t.Id })
                .ToListAsync();

            modelBaseList = modelBaseList.GroupJoin(deleteList,
               a => a.DeleteId,
               b => b.Id,
               (a, b) =>
               {
                   a.DeleteBy = b.FirstOrDefault()?.NickName;
                   return a;
               }).ToList();

            var dataList = modelBaseList.Cast<T>().ToList(); ;
            return OperateResult.CreateSuccessResult(dataList);
        }

        /// <summary>
        /// 分页查询数据并且返回创建人 修改人 和删除人
        /// </summary>
        /// <typeparam name="TEntity">数据实体</typeparam>
        /// <typeparam name="TResult">返回实体</typeparam>
        /// <param name="pagination">分页实体</param>
        /// <param name="paginationCallBack">分页回调</param>
        /// <returns></returns>
        public async Task<OperateResult<PageDataModel<TResult>>> GetPaginationListWithCreateUpateDeleteByAsync<TEntity, TResult>(Pagination pagination,
            Func<List<TResult>, Task<List<TResult>>> paginationCallBack = null)
            where TEntity : class
            where TResult : ModelBase, new()
        {
            var getPaginationListReuslt = await GetPaginationListAsync<TEntity, TResult>(pagination, paginationCallBack);
            if (!getPaginationListReuslt.IsSuccess)
            {
                return getPaginationListReuslt;
            }
            var pageDataModel = getPaginationListReuslt.Data;
            var modelBaseList = pageDataModel.DataList.Cast<ModelBase>().ToList();
            var operateResult = await this.GetCreateUpdateDeleteByAsync<TResult>(modelBaseList);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<PageDataModel<TResult>>(operateResult);
            }
            pageDataModel.DataList = operateResult.Data;
            return OperateResult.CreateSuccessResult(pageDataModel);
        }

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <typeparam name="TEntity">数据实体</typeparam>
        /// <typeparam name="TResult">返回实体</typeparam>
        /// <param name="pagination">分页实体</param>
        /// <param name="paginationCallBack">分页回调</param>
        /// <returns></returns>
        public async Task<OperateResult<PageDataModel<TResult>>> GetPaginationListAsync<TEntity, TResult>(Pagination pagination,
           Func<List<TResult>, Task<List<TResult>>> paginationCallBack = null)
           where TEntity : class
           where TResult : ModelBase, new()
        {
            PageDataModel<TResult> pageDataModel = new PageDataModel<TResult>();

            bool isAsc = pagination.sord != null && pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx != null ? pagination.sidx.Split(',') : new string[] { };

            //在这里拼接EF Lambda查询语句
            var tempData = this.RSAppDb.Set<TEntity>().AsQueryable();
            foreach (string item in _order)
            {
                MethodCallExpression resultExp = null;
                string _orderPart = item;
                _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                string[] _orderArry = _orderPart.Trim().Split(' ');
                string _orderField = _orderArry[0];
                bool sort = isAsc;
                if (_orderArry.Length == 2)
                {
                    isAsc = _orderArry[1].ToUpper() == "ASC" ? true : false;
                }
                var parameter = Expression.Parameter(typeof(TEntity), "t");
                var property = typeof(TEntity).GetProperty(_orderField);
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                resultExp = Expression.Call(typeof(Queryable),
                    isAsc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(TEntity),
                    property.PropertyType },
                    tempData.Expression,
                    Expression.Quote(orderByExp));
                tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
            }

            //获取总行数
            //在这里可能会存在问题，比如数据非常多的时候通过这个方法会造成影响
            pagination.records = await tempData.CountAsync();

            List<TEntity> dataList = await tempData
                                  .Skip<TEntity>(pagination.rows * (pagination.page - 1))
                                  .Take<TEntity>(pagination.rows)
                                  .ToListAsync();

            var resultList = dataList.Select(source =>
            {
                TResult target = new TResult();
                var targetType = target.GetType();
                var sourceType = source.GetType();

                // 获取源对象的所有公共属性
                var sourceProperties = sourceType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var sourceProperty in sourceProperties)
                {
                    // 检查目标对象是否有同名且类型相同的属性
                    var targetProperty = targetType.GetProperty(sourceProperty.Name);
                    if (targetProperty != null &&
                        targetProperty.CanWrite &&
                        targetProperty.PropertyType == sourceProperty.PropertyType &&
                        sourceProperty.CanRead)
                    {
                        // 获取源属性的值并设置到目标属性
                        var value = sourceProperty.GetValue(source);
                        targetProperty.SetValue(target, value);
                    }
                }
                return target;
            }).ToList();

            if (paginationCallBack != null)
            {
                resultList = await paginationCallBack.Invoke(resultList);
            }
            pageDataModel.DataList = resultList;
            pageDataModel.Pagination = pagination;
            return OperateResult.CreateSuccessResult(pageDataModel);
        }


        /// <summary>
        /// 动态通过DTO生成实体类表达式树
        /// </summary>
        /// <typeparam name="TDto">参数类型</typeparam>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <param name="dto">客户端传递过来的参数</param>
        /// <returns></returns>
        public Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>
        CreateSetPropertiesExpression<TDto, TEntity>(TDto dto)
        {
            var settersParam = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "setters");
            Expression? body = settersParam;

            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);

            var dtoProperties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var dtoProp in dtoProperties)
            {
                var value = dtoProp.GetValue(dto);

                // 查找实体中同名属性
                var entityProp = entityType.GetProperty(dtoProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (entityProp == null) continue;
                if (!entityProp.CanWrite) continue;
                if (!entityProp.PropertyType.IsAssignableFrom(dtoProp.PropertyType)) continue;

                object? setValue = value;

                // string类型，空或空白时，setValue设为null
                if (dtoProp.PropertyType == typeof(string))
                {
                    if (value is string str && string.IsNullOrWhiteSpace(str))
                    {
                        setValue = null;
                    }
                }
                else
                {
                    // 其它类型，null时跳过
                    if (value == null) continue;
                }

                // b => b.Prop
                var bParam = Expression.Parameter(entityType, "b");
                var propertyAccess = Expression.Property(bParam, entityProp);
                var propertyLambda = Expression.Lambda(propertyAccess, bParam);

                // setters.SetProperty(b => b.Prop, setValue)
                var setPropertyMethod = typeof(SetPropertyCalls<TEntity>)
                    .GetMethod("SetProperty", new[] {
                typeof(Expression<>)
                .MakeGenericType(typeof(Func<,>)
                .MakeGenericType(entityType, entityProp.PropertyType)),
                entityProp.PropertyType
                    });

                var valueConst = Expression.Constant(setValue, entityProp.PropertyType);

                body = Expression.Call(
                    body!,
                    setPropertyMethod!,
                    propertyLambda,
                    valueConst
                );
            }

            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body!, settersParam);
        }


        /// <summary>
        /// 动态创建表达式树
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体参数</param>
        /// <returns></returns>
        public Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>
            CreateSetPropertiesExpression<T>(T entity) where T : class
        {
            var settersParam = Expression.Parameter(typeof(SetPropertyCalls<T>), "setters");
            Expression? body = settersParam;

            var entityType = typeof(T);
            var properties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                if (value == null) continue;

                // b => b.Prop
                var bParam = Expression.Parameter(entityType, "b");
                var propertyAccess = Expression.Property(bParam, prop);
                var propertyLambda = Expression.Lambda(propertyAccess, bParam);

                // setters.SetProperty(b => b.Prop, value)
                var setPropertyMethod = typeof(SetPropertyCalls<T>)
                    .GetMethod("SetProperty", new[] {
                    typeof(Expression<>)
                    .MakeGenericType(typeof(Func<,>)
                    .MakeGenericType(entityType, prop.PropertyType)),
                    prop.PropertyType
                    });

                var valueConst = Expression.Constant(value, prop.PropertyType);

                body = Expression.Call(
                    body!,
                    setPropertyMethod!,
                    propertyLambda,
                    valueConst
                );
            }

            return Expression.Lambda<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>(body!, settersParam);
        }

    }
}
