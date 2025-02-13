using RS.Commons.Compares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    public static class LinqExtension
    {
        public static List<T> HashDistinct<T>(this List<T> dataList)
        {
            return dataList.Distinct(new ReflectionComparer<T>()).ToList();
        }

        public static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> collection, List<string> propertyList = null)
        {
            // 分组查找重复项
            return collection
               .GroupBy(item => item.GetHashCode<T>(propertyList))
               .Where(group => group.Count() > 1)
               .SelectMany(group => group);
        }



        public static int GetHashCode<T>(this T obj, List<string> propertyList = null)
        {
            if (obj == null)
            {
                return 0;
            }
            int hash = 17;
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties = properties?.Join(propertyList, a => a.Name, b => b, (a, b) => a)?.ToArray();
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);
                    hash = hash * 23 + (value?.GetHashCode() ?? 0);
                }
            }
            return hash;
        }
    }
}
