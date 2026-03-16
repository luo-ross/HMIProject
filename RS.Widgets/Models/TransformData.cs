using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 变换数据模型，用于绑定同步视觉状态
    /// </summary>
    public class TransformData : NotifyBase
    {
        private double x;
        public double X
        {
            get
            {
                return x;
            }
            set
            {
                SetProperty(ref x, value);
            }
        }

        private double y;
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                SetProperty(ref y, value);
            }
        }

        private double width;
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                SetProperty(ref width, value);
            }
        }

        private double height;
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                SetProperty(ref height, value);
            }
        }

        private double angle;
        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                SetProperty(ref angle, value);
            }
        }

        private RectDirection direction;
        public RectDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                SetProperty(ref direction, value);
            }
        }

        private Point topLeft;
        public Point TopLeft
        {
            get
            {
                return topLeft;
            }
            set
            {
                SetProperty(ref topLeft, value);
            }
        }

        private Point topRight;
        public Point TopRight
        {
            get
            {
                return topRight;
            }
            set
            {
                SetProperty(ref topRight, value);
            }
        }

        private Point bottomLeft;
        public Point BottomLeft
        {
            get
            {
                return bottomLeft;
            }
            set
            {
                SetProperty(ref bottomLeft, value);
            }
        }

        private Point bottomRight;
        public Point BottomRight
        {
            get
            {
                return bottomRight;
            }
            set
            {
                SetProperty(ref bottomRight, value);
            }
        }

        private double pivotX = 0.5;
        public double PivotX
        {
            get
            {
                return pivotX;
            }
            set
            {
                SetProperty(ref pivotX, value);
            }
        }

        private double pivotY = 0.5;
        public double PivotY
        {
            get
            {
                return pivotY;
            }
            set
            {
                SetProperty(ref pivotY, value);
            }
        }

        private Point pivot;
        public Point Pivot
        {
            get
            {
                return pivot;
            }
            set
            {
                SetProperty(ref pivot, value);
            }
        }
    }
}
