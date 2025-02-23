using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.ClientData.Entities
{
    /// <summary>
    /// 点位采集数据
    /// </summary>
    public sealed class DataSource
    {
        /// <summary>
        /// 数据库主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 这里绑定的是通讯配置数据主键
        /// </summary>
        public int CommuConfigId { get; set; }

        /// <summary>
        /// 采集的数据
        /// </summary>
        public double Data { get; set; }

        /// <summary>
        /// 数据采集时间 这里用long类型
        /// 表示从1970年1月1日0时0分0秒到现在的毫秒数
        /// </summary>
        public long DataTime { get; set; }
    }
}
