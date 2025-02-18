using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    public static class ConversionExtensions
    {
        /// <summary>
        /// 将对象转换为 int 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 int 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 int 类型时抛出</exception>
        public static int ToInt(this object obj)
        {
            if (obj == null) return 0;
            if (int.TryParse(obj.ToString(), out int result)) return result;
            return default(int);
        }

        /// <summary>
        /// 将对象转换为 short 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 short 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 short 类型时抛出</exception>
        public static short ToShort(this object obj)
        {
            if (obj == null) return 0;
            if (short.TryParse(obj.ToString(), out short result)) return result;
            return default(short);
        }

        /// <summary>
        /// 将对象转换为 byte 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 byte 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 byte 类型时抛出</exception>
        public static byte ToByte(this object obj)
        {
            if (obj == null) return 0;
            if (byte.TryParse(obj.ToString(), out byte result)) return result;
            return default(byte);
        }

        /// <summary>
        /// 将对象转换为 double 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 double 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 double 类型时抛出</exception>
        public static double ToDouble(this object obj)
        {
            if (obj == null) return 0;
            if (double.TryParse(obj.ToString(), out double result)) return result;
            return default(double);
        }

        /// <summary>
        /// 将对象转换为 float 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 float 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 float 类型时抛出</exception>
        public static float ToFloat(this object obj)
        {
            if (obj == null) return 0;
            if (float.TryParse(obj.ToString(), out float result)) return result;
            return default(float);
        }

        /// <summary>
        /// 将对象转换为 decimal 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 decimal 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 decimal 类型时抛出</exception>
        public static decimal ToDecimal(this object obj)
        {
            if (obj == null) return 0;
            if (decimal.TryParse(obj.ToString(), out decimal result)) return result;
            return default(decimal);
        }

        /// <summary>
        /// 将对象转换为 long 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 long 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 long 类型时抛出</exception>
        public static long ToLong(this object obj)
        {
            if (obj == null) return 0;
            if (long.TryParse(obj.ToString(), out long result)) return result;
            return default(long);
        }

        /// <summary>
        /// 将对象转换为 sbyte 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 sbyte 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 sbyte 类型时抛出</exception>
        public static sbyte ToSByte(this object obj)
        {
            if (obj == null) return 0;
            if (sbyte.TryParse(obj.ToString(), out sbyte result)) return result;
            return default(sbyte);
        }

        /// <summary>
        /// 将对象转换为 ushort 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 ushort 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 ushort 类型时抛出</exception>
        public static ushort ToUShort(this object obj)
        {
            if (obj == null) return 0;
            if (ushort.TryParse(obj.ToString(), out ushort result)) return result;
            return default(ushort);
        }

        /// <summary>
        /// 将对象转换为 uint 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 uint 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 uint 类型时抛出</exception>
        public static uint ToUInt(this object obj)
        {
            if (obj == null) return 0;
            if (uint.TryParse(obj.ToString(), out uint result)) return result;
            return default(uint);
        }

        /// <summary>
        /// 将对象转换为 ulong 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 ulong 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 ulong 类型时抛出</exception>
        public static ulong ToULong(this object obj)
        {
            if (obj == null) return 0;
            if (ulong.TryParse(obj.ToString(), out ulong result)) return result;
            return default(ulong);
        }

        /// <summary>
        /// 将对象转换为 bool 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 bool 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 bool 类型时抛出</exception>
        public static bool ToBool(this object obj)
        {
            if (obj == null) return false;
            if (bool.TryParse(obj.ToString(), out bool result)) return result;
            if (int.TryParse(obj.ToString(), out int intResult))
            {
                return intResult != 0;
            }
            return default(bool);
        }

        /// <summary>
        /// 将对象转换为 char 类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的 char 值，如果转换失败会抛出异常</returns>
        /// <exception cref="InvalidCastException">当无法将对象转换为 char 类型时抛出</exception>
        public static char ToChar(this object obj)
        {
            if (obj == null) return '\0';
            if (char.TryParse(obj.ToString(), out char result)) return result;
            return default(char);
        }

        /// <summary>
        /// 将对象转换为指定的枚举类型
        /// </summary>
        /// <typeparam name="TEnum">目标枚举类型</typeparam>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的枚举值，如果转换失败会抛出异常</returns>
        /// <exception cref="ArgumentException">当 TEnum 不是枚举类型时抛出</exception>
        /// <exception cref="InvalidCastException">当无法将对象转换为指定枚举类型时抛出</exception>
        public static TEnum ToEnum<TEnum>(this object obj) where TEnum : struct, Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum 必须是枚举类型。");
            }

            if (obj == null)
            {
                return default;
            }

            if (Enum.TryParse(obj.ToString(), true, out TEnum result))
            {
                return result;
            }
            return default(TEnum);
        }
    }
}
