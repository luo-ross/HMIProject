using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.AnnotationSQLite.Entities
{
    public class Tags
    {
        /// <summary>
        /// 标签唯一主键
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// 所属项目
        /// </summary>
        public string ProjectId { get; set; }



        /// <summary>
        /// 类别名称
        /// </summary>
        public string? ClassName { get; set; }


        /// <summary>
        /// 类别颜色
        /// </summary>
        public string? TagColor { get; set; }


        /// <summary>
        /// 标签快捷键
        /// </summary>
        public string? ShortCut { get; set; }


        /// <summary>
        /// 是否自动快捷键
        /// </summary>
        public bool  IsShortCutAuto { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect { get; set; }
       
    }
}
