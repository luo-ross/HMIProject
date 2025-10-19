using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Extensions
{
    public static class LinqExtensions
    {

        /// <summary>
        /// 对目标列表先筛选，再根据参照列表的顺序排序（支持不同类型）
        /// </summary>
        /// <typeparam name="TTarget">目标列表元素类型</typeparam>
        /// <typeparam name="TReference">参照列表元素类型</typeparam>
        /// <typeparam name="TKey">关联键类型（两种元素共有的关联字段类型）</typeparam>
        /// <param name="targetList">需要排序的目标列表</param>
        /// <param name="referenceList">作为排序依据的参照列表</param>
        /// <param name="targetKeySelector">从目标元素中提取关联键的委托</param>
        /// <param name="referenceKeySelector">从参照元素中提取关联键的委托</param>
        /// <param name="filter">筛选目标列表元素的条件（可选）</param>
        /// <returns>筛选并排序后的新列表</returns>
        public static List<TTarget> SortByReferenceList<TTarget, TReference, TKey>(
            this List<TTarget> targetList,
            List<TReference> referenceList,
            Func<TTarget, TKey> targetKeySelector,
            Func<TReference, TKey> referenceKeySelector,
            Func<TTarget, bool> filter = null)
        {

            var filteredTargets = filter != null
                ? targetList.Where(filter).ToList()
                : new List<TTarget>(targetList);
          
            var referenceKeyIndexDict = referenceList
                .Select((item, index) => new { Key = referenceKeySelector(item), Index = index })
                .ToDictionary(pair => pair.Key, pair => pair.Index);

            return filteredTargets
                .OrderBy(target =>
                    referenceKeyIndexDict.TryGetValue(targetKeySelector(target), out int index)
                        ? index
                        : int.MaxValue)
                .ToList();
        }
    }
}
