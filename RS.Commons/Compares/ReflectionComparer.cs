using RS.Commons.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RS.Commons.Compares
{
    public class ReflectionComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            // 获取所有公共属性
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var valueX = property.GetValue(x);
                var valueY = property.GetValue(y);

                if (!object.Equals(valueX, valueY))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode<T>();
        }
    }
}
