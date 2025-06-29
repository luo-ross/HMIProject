using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{
    public static class ObservableCollectionExtensions
    {
        public static void InsertRange<T>(this ObservableCollection<T> collection, int index, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Insert(index++, item);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }

        /// <summary>
        /// 根据ParentId递归删除所有子节点（Id为string类型，防止死循环）
        /// </summary>
        public static void RemoveRangeWithCascade<T>(
            this ObservableCollection<T> collection,
            IEnumerable<T> items,
            Func<T, string> idSelector,
            Func<T, string> parentIdSelector)
        {
            var toRemove = new HashSet<string>(items.Select(idSelector));
            var queue = new Queue<string>(toRemove);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                // 找到所有直接子节点
                var children = collection
                    .Where(x => {
                        var pid = parentIdSelector(x);
                        return !string.IsNullOrEmpty(pid) && pid == currentId && !toRemove.Contains(idSelector(x));
                    })
                    .ToList();

                foreach (var child in children)
                {
                    var childId = idSelector(child);
                    if (toRemove.Add(childId)) // 只处理未处理过的节点
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            // 最后统一移除
            var allToRemove = collection.Where(x => toRemove.Contains(idSelector(x))).ToList();
            foreach (var item in allToRemove)
            {
                collection.Remove(item);
            }
        }
    }
}
