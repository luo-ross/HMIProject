using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIClient.Models
{
    public enum DataType
    {
        // 表示布尔类型
        Boolean = 3,
        // 表示 16 位有符号整数类型，对应 short
        Short = 7,
        // 表示 16 位无符号整数类型，对应 ushort
        UShort = 8,
        // 表示 32 位有符号整数类型，对应 int
        Int = 9,
        // 表示 32 位无符号整数类型，对应 uint
        UInt = 10,
        // 表示 64 位有符号整数类型，对应 long
        Long = 11,
        // 表示 64 位无符号整数类型，对应 ulong
        ULong = 12,
        // 表示单精度浮点数类型，对应 float
        Float = 13,
        // 表示双精度浮点数类型，对应 double
        Double = 14,
        // 表示字符串类型，对应 string
        String = 18
    }
}
