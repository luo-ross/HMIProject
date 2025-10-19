using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.SQLite.Entities
{
    public class Rects
    {
        /// <summary>
        /// 矩形唯一主键
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// 所属项目
        /// </summary>
        public string ProjectId { get; set; }


        /// <summary>
        /// 所属图像
        /// </summary>
        public long PictureId { get; set; }

        /// <summary>
        /// 所属标签类别
        /// </summary>
        public string TagId { get; set; }

        /// <summary>
        /// 矩形左上角X坐标
        /// </summary>
        public double CanvasLeft { get; set; }


        /// <summary>
        /// 矩形左上角y坐标
        /// </summary>
        public double CanvasTop { get; set; }

        /// <summary>
        /// 矩形宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 矩形高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 矩形旋转角度
        /// </summary>
        public double Angle { get; set; }
    }
}
