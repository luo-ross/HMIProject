using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.AnnotationSQLite.Entities
{
    public sealed class Projects
    {
        /// <summary>
        /// 项目主键
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 项目文件路径
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string  Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public long UpdateTime { get; set; }
    }
}
