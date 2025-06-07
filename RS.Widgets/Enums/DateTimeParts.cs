using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Enums
{
    // 使用 [Flags] 属性标记该枚举支持按位组合
    [Flags]
    public enum DateTimeParts
    {
        None = 0,           // 无（默认值）
        Year = 1 << 0,      // 年 (2^0 = 1)
        Month = 1 << 1,     // 月 (2^1 = 2)
        Day = 1 << 2,       // 日 (2^2 = 4)
        Hour = 1 << 3,      // 时 (2^3 = 8)
        Minute = 1 << 4,    // 分 (2^4 = 16)
        Second = 1 << 5,    // 秒 (2^5 = 32)

        // 预定义常用组合
        Date = Year | Month | Day,               // 年月日
        Time = Hour | Minute | Second,           // 时分秒
        DateTime = Date | Time                   // 完整日期时间
    }
}
