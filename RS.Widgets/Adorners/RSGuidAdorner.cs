using Microsoft.Extensions.Logging;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Extensions;
using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Adorners
{
    public class RSGuidAdorner : Adorner
    {
        public RSGuidAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            this.DrawPen = new Pen()
            {
                Brush = Brushes.DimGray,
                Thickness = 2,
                //DashStyle = new DashStyle(new double[] { 4, 4 }, 0)
            };

            this.MouseMove += RSGuidAdorner_MouseMove;
        }

        private void RSGuidAdorner_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("RSGuidAdorner_MouseMove");
        }

        public Pen DrawPen
        {
            get { return (Pen)GetValue(DrawPenProperty); }
            set { SetValue(DrawPenProperty, value); }
        }

        public static readonly DependencyProperty DrawPenProperty =
            DependencyProperty.Register("DrawPen", typeof(Pen), typeof(RSNavListSortAdorner), new PropertyMetadata(null));


        private Point StartPoint;
        private Point EndPoint;

        public void UpdateGuideLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (StartPoint != default || EndPoint != default)
            {
                this.DrawGuideLine(drawingContext, StartPoint, EndPoint);
            }
        }


        private void DrawGuideLine(DrawingContext drawingContext, Point startPoint, Point endPoint)
        {
            double triangleBase = 5;   // 三角形底边长度
            double triangleHeight = 5; // 三角形高

            Vector direction = endPoint - startPoint;
            direction.Normalize();
            Vector perp = new Vector(-direction.Y, direction.X);
            Point realStart = startPoint + direction * triangleHeight;
            Point realEnd = endPoint - direction * triangleHeight;
            drawingContext.DrawLine(this.DrawPen, realStart, realEnd);
            DrawArrowTriangle(drawingContext, startPoint, direction, perp, triangleBase, triangleHeight);
            DrawArrowTriangle(drawingContext, endPoint, -direction, perp, triangleBase, triangleHeight);
        }


        private void DrawArrowTriangle(DrawingContext dc, Point point, Vector direction, Vector perp, double baseLen, double height)
        {
            Point tip = point + direction * height;
            Point baseLeft = point + perp * (baseLen / 2);
            Point baseRight = point - perp * (baseLen / 2);
            StreamGeometry geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true, true);
                ctx.LineTo(baseLeft, true, false);
                ctx.LineTo(baseRight, true, false);
            }
            geometry.Freeze();
            dc.DrawGeometry(this.DrawPen.Brush, this.DrawPen, geometry);
        }
    }



}
