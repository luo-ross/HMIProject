using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.AnnotationSQLite.Entities
{
    public sealed class Pictures
    {
        /// <summary>
        /// 图像唯一主键
        /// </summary>
        public long Id { get; set; }


        /// <summary>
        /// 所属项目
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// 图像名称
        /// </summary>
        public string? ImgName { get; set; }

        /// <summary>
        /// 图像路径
        /// </summary>
        public string?  ImgPath { get; set; }


        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect { get; set; }

        /// <summary>
        /// 是否工作
        /// </summary>
        public bool IsWroking { get; set; }
    }
}
