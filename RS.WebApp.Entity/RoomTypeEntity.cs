using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 房型信息
    /// </summary>
    public sealed class RoomTypeEntity 
    {
        /// <summary>
        /// 房型编码
        /// </summary>
        [Key]
        public string Code { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        public string? ChName { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string? EnName { get; set; }

    }
}