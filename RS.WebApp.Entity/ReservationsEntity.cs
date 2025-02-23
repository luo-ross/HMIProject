using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 预定单
    /// </summary>
    public sealed class ReservationsEntity : BaseEntity
    {

        /// <summary>
        /// 预定姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 预抵时间
        /// </summary>
        public long? Arrival { get; set; }

        /// <summary>
        /// 离店时间
        /// </summary>
        public long? Departure { get; set; }

        /// <summary>
        /// 成人数量
        /// </summary>
        public int? Adults { get; set; }

        /// <summary>
        /// 小孩数量
        /// </summary>
        public int? Children { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string? Company { get; set; }

        /// <summary>
        /// 预定状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 平均房价
        /// </summary>
        public decimal? AverageRate { get; set; }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal? SumRate { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 预定号
        /// </summary>
        public string? ReservationNo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long? CreateTime { get; set; }

        /// <summary>
        /// 房间数量
        /// </summary>
        public int? Rooms { get; set; }

        /// <summary>
        /// 父级 用作拆分订单使用
        /// </summary>
        public long? Parent { get; set; }





    }
}