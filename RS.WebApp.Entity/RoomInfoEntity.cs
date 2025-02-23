using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 客房信息
    /// </summary>
    [PrimaryKey("RoomNo", "Floor")]
    public sealed class RoomInfoEntity 
    {
        /// <summary>
        /// 房号
        /// </summary>
        [Key]
        public string RoomNo { get; set; }

        /// <summary>
        /// 房型
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 楼层
        /// </summary>
        [Key]
        public string Floor { get; set; }

        /// <summary>
        /// 房态
        /// </summary>
        public string? Status { get; set; }
    }
}