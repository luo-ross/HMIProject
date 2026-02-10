using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{
    /// <summary>
    /// 排序顺序枚举
    /// </summary>
    public enum EmailSortType
    {
        /// <summary>
        /// 由新到旧（按日期降序排列，最新的在前）
        /// </summary>
        NewestToOldest,

        /// <summary>
        /// 由旧到新（按日期升序排列，最旧的在前）
        /// </summary>
        OldestToNewest,

        /// <summary>
        /// 由大到小（按大小降序排列，最大的在前）
        /// </summary>
        LargeToSmall,

        /// <summary>
        /// 由小到大（按大小升序排列，最小的在前）
        /// </summary>
        SmallToLarge
    }
}

