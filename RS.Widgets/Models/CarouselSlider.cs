using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class CarouselSlider
    {
        public string Name { get; set; }

        public string Caption { get; set; }

        public string Description { get; set; }

        public string ImageSource { get; set; }

        public string Location { get; set; }

        /// <summary>
        /// 这里是16进制带#的Hex颜色字符串 比如#fff
        /// </summary>
        public string Background { get; set; }
    }
}
