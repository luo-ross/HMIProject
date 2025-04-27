using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 矩形框
    /// </summary>
    public class RectModel
    {
        public RectModel(int left, int top, int width, int height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

    }
}
