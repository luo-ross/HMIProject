using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RS.Widgets.Models
{
    public class CarouselSlider
    {
        /// <summary>
        /// 记录索引
        /// </summary>
        public int Index { get; set; }
        public string Name { get; set; }

        public string Caption { get; set; }

        public string Description { get; set; }

        public string ImageSource { get; set; }

        public string Location { get; set; }

        public double Scale { get; set; } = 1;

        /// <summary>
        /// 缩放后的宽度差
        /// </summary>
        public double ScaleWidthDif { get; set; }

        public double TransformX { get; set; }

        public double CanvasLeft { get; set; }

        public double CanvasTop { get; set; }

        public double CenterX { get; set; }
        public double CenterY { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
        public int ZIndex { get; set; }

        /// <summary>
        /// 这里是16进制带#的Hex颜色字符串 比如#fff
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// 模糊程度
        /// </summary>
        public double Blur { get; set; }

        public RSCarouselSlider RSCarouselSlider { get; set; }

        public ScaleTransform ScaleTransform { get; set; }

 
        public TranslateTransform3D TranslateTransform3D { get; set; }
    }
}
