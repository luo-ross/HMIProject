using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RS.Commons.Extend;

namespace RS.Commons.Web
{
    /// <summary>
    /// 该类只要用于构造Jqgrid的树状插叙 核心代码 方法从网络收集整理 
    /// </summary>
    public static class TreeQuery
    {
        /// <summary>
        /// 该方法用构造Jqgrid的树状递归查询 具体使用放可以查看该方法的引用实例 
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="entityList">实体类List</param>
        /// <param name="condition">Lamda条件</param>
        /// <param name="keyValue">主键值</param>
        /// <param name="parentId">父键值</param>
        /// <returns></returns>
        public static List<T> TreeWhere<T>(this List<T> entityList, Predicate<T> condition, string keyValue = "Id", string parentId = "ParentId") where T : class
        {
            List<T> locateList = entityList.FindAll(condition);
            var parameter = Expression.Parameter(typeof(T), "t");
            List<T> treeList = new List<T>();
            foreach (T entity in locateList)
            {
                treeList.Add(entity);
                string pId = entity.GetType().GetProperty(parentId).GetValue(entity, null)?.ToString();
                while (true)
                {
                    if (string.IsNullOrEmpty(pId) && pId == "0")
                    {
                        break;
                    }
                    Predicate<T> upLambda = (Expression.Equal(parameter.Property(keyValue), Expression.Constant(pId))).ToLambda<Predicate<T>>(parameter).Compile();
                    T upRecord = entityList.Find(upLambda);
                    if (upRecord != null)
                    {
                        treeList.Add(upRecord);
                        pId = upRecord.GetType().GetProperty(parentId).GetValue(upRecord, null).ToString();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return treeList.Distinct().ToList();
        }

        /// <summary>
        /// 该方法用于生成Jqgrid表单的树查询 
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="entityList">实体List</param>
        /// <param name="treeList">所要生成的实体List</param>
        /// <param name="condition">Lambda判断条件</param>
        /// <param name="keyValue">主键值</param>
        /// <param name="parentId">父键值</param>
        /// <returns></returns>
        public static List<T> TreeList<T>(this List<T> entityList, List<T> treeList, Predicate<T> condition, string keyValue = "Id", string parentId = "ParentId") where T : class
        {
            List<T> locateList = entityList.FindAll(condition);
            var parameter = Expression.Parameter(typeof(T), "t");
            foreach (T entity in locateList)
            {
                treeList.Add(entity);
                string Id = entity.GetType().GetProperty(keyValue).GetValue(entity, null).ToString();
                Predicate<T> upLambda = (Expression.Equal(parameter.Property(parentId), Expression.Constant(Id))).ToLambda<Predicate<T>>(parameter).Compile();
                entityList.TreeList(treeList, upLambda);
            }
            return treeList.Distinct().ToList();
        }
    }
}
