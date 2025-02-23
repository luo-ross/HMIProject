using System;
using System.ComponentModel;

namespace RS.Commons.Extend
{
    public static class ObjectExtend
    {
        /// <summary>
        /// 类型转换 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object AsType(this object value, Type conversionType)
        {
            if (value != null && conversionType == value.GetType())
                return value;

            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }

            if (conversionType == typeof(Guid))
            {
                return Guid.Parse(value + "");
            }

            try
            {
                return Convert.ChangeType(value, conversionType);
            }
            catch
            {
                return null;
            }
        }

    }
}
