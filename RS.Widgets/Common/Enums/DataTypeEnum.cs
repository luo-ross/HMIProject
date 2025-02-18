using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public enum DataTypeEnum
    {
        // 表示布尔类型
        Bool = 0,
        // 表示 16 位有符号整数类型，对应 short
        Short = 1,
        // 表示 16 位无符号整数类型，对应 ushort
        UShort = 2,
        // 表示 32 位有符号整数类型，对应 int
        Int = 3,
        // 表示 32 位无符号整数类型，对应 uint
        UInt = 4,
        // 表示 64 位有符号整数类型，对应 long
        Long = 5,
        // 表示 64 位无符号整数类型，对应 ulong
        ULong = 6,
        // 表示单精度浮点数类型，对应 float
        Float = 7,
        // 表示双精度浮点数类型，对应 double
        Double = 8,
        // 表示字符串类型，对应 string
        String = 9
    }
}
