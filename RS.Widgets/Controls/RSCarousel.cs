using Castle.Core.Logging;
using ScottPlot.Statistics;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkiaSharp;
using RS.Widgets.Models;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Controls.Primitives;
using NPOI.SS.Formula.Functions;
using NPOI.POIFS.Properties;

namespace RS.Widgets.Controls
{
    public class RSCarousel : RSUserControl
    {
        private Canvas PART_Canvas;
        static RSCarousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCarousel), new FrameworkPropertyMetadata(typeof(RSCarousel)));
        }

        public RSCarousel()
        {
            this.Loaded += RSCarousel_Loaded;
        }

        private void RSCarousel_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefeshCarousel();
        }



        [Description("轮播图宽度")]
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(double), typeof(RSCarousel), new PropertyMetadata(300D));




        [Description("轮播图高度")]
        public double SliderHeight
        {
            get { return (double)GetValue(SliderHeightProperty); }
            set { SetValue(SliderHeightProperty, value); }
        }

        public static readonly DependencyProperty SliderHeightProperty =
            DependencyProperty.Register("SliderHeight", typeof(double), typeof(RSCarousel), new PropertyMetadata(400D));





        [Description("轮播数据集")]
        public ObservableCollection<CarouselSlider> CarouselSliderList
        {
            get { return (ObservableCollection<CarouselSlider>)GetValue(CarouselSliderListProperty); }
            set { SetValue(CarouselSliderListProperty, value); }
        }

        public static readonly DependencyProperty CarouselSliderListProperty =
            DependencyProperty.Register("CarouselSliderList", typeof(ObservableCollection<CarouselSlider>), typeof(RSCarousel), new PropertyMetadata(null, CarouselSliderListPropertyChanged));

        private static void CarouselSliderListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsCarousel = d as RSCarousel;

            //根据用户传递过来的数据需要处理一下
            rsCarousel.DataInit();
            rsCarousel?.RefeshCarousel();
        }


        private void DataInit()
        {
            var count = this.CarouselSliderList.Count;

            int middleIndex = this.CarouselSliderList.Count / 2;
            // 获取前半部分
            this.LeftDataList = this.CarouselSliderList.Take(middleIndex).ToList();
            // 获取后半部分
            this.RightDataList = this.CarouselSliderList.Skip(middleIndex).ToList();
        }

        /// <summary>
        /// 缩放后的轮廓图片高度差
        /// </summary>
        public int SliderScaleHeightDiff = 60;


        private List<CarouselSlider> RightDataList = new List<CarouselSlider>();
        private List<CarouselSlider> LeftDataList = new List<CarouselSlider>();

        private void RefeshCarousel()
        {
            if (this.CarouselSliderList == null)
            {
                return;
            }
            if (this.PART_Canvas == null)
            {
                return;
            }

            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;
            //获取Slider的长宽

            var viewCenterX = canvasWidth / 2;
            var viewCenterY = canvasHeight / 2;

            int middleIndex = this.CarouselSliderList.Count / 2;

            //直线方程的K值
            double k = SliderScaleHeightDiff / this.SliderWidth; // 斜率等于对边比邻边

            var canvasTop = canvasHeight / 2 - this.SliderHeight / 2;
            var x = -this.SliderWidth / 2;
            var y = this.SliderHeight;
            var b = y - k * x;
            //得到整个图像最大长度的一半
            var halfLength = Math.Abs(-b / k);

            for (int i = 0; i < this.CarouselSliderList.Count; i++)
            {

                var item = this.CarouselSliderList[i];
                item.Width = this.SliderWidth;
                item.Height = this.SliderHeight;
                item.CanvasTop = canvasTop;
                double percent = 0;
                double transferX = 0;
                if (i < middleIndex)
                {
                    var distanceFromMiddle = Math.Abs(i - middleIndex) * item.Width + item.Width * 0.5;
                    item.ZIndex = i;
                    item.CanvasLeft = viewCenterX - distanceFromMiddle;
                    percent = (distanceFromMiddle - item.Width * 0.5) / halfLength;
                }
                else if (i == middleIndex)
                {
                    item.ZIndex = i;
                    item.CanvasLeft = viewCenterX - item.Width * 0.5;
                }
                else
                {
                    item.ZIndex = this.CarouselSliderList.Count - i - 1;
                    item.CanvasLeft = viewCenterX + Math.Abs(i - middleIndex - 1) * item.Width + item.Width * 0.5;
                    percent = Math.Abs(i - middleIndex) * item.Width / halfLength;
                }
                item.Scale = this.MaxScale - percent;
                item.Scale = Math.Max(this.MinScale, item.Scale);

                var widthScale = item.Width * item.Scale;
                item.ScaleWidthDif = item.Width - widthScale;
            }

          

          

            foreach (var item in this.CarouselSliderList)
            {
                if (item.RSCarouselSlider == null)
                {
                    RSCarouselSlider rsCarouselSlider = new RSCarouselSlider();
                    rsCarouselSlider.Name = item.Name;
                    rsCarouselSlider.Background = (Brush)new BrushConverter().ConvertFrom(item.Background);
                    rsCarouselSlider.BlurRadius = 5;
                    rsCarouselSlider.Caption = item.Caption;
                    rsCarouselSlider.Description = item.Description;
                    rsCarouselSlider.ImageSource = item.ImageSource;
                    rsCarouselSlider.Location = item.Location;
                    rsCarouselSlider.DragDelta += RsCarouselSlider_DragDelta;
                    rsCarouselSlider.Width = this.SliderWidth;
                    rsCarouselSlider.Height = this.SliderHeight;
                    rsCarouselSlider.Tag = item;
                    item.RSCarouselSlider = rsCarouselSlider;
                    Canvas.SetLeft(rsCarouselSlider, item.CanvasLeft);
                    Canvas.SetTop(rsCarouselSlider, item.CanvasTop);
                    Panel.SetZIndex(rsCarouselSlider, item.ZIndex);
                    this.PART_Canvas.Children.Add(item.RSCarouselSlider);

                }

                TransformGroup transformGroup = new TransformGroup();
                item.RSCarouselSlider.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform scaleTransform = new ScaleTransform()
                {
                    ScaleX = item.Scale,
                    ScaleY = item.Scale,
                };
                transformGroup.Children.Add(scaleTransform);
                //TranslateTransform translateTransform = new TranslateTransform()
                //{
                //    X = transformX,
                //};
                //transformGroup.Children.Add(translateTransform);
                item.RSCarouselSlider.RenderTransform = transformGroup;
            }

            //double canvasLeft = 0;
            //double transformXTotal = 0;

            //for (int i = 0; i < RightDataList.Count; i++)
            //{
            //    var item = RightDataList[i];
            //    //如果是最中间这个不进行模糊
            //    if (i == 0)
            //    {
            //        item.RSCarouselSlider.BlurRadius = 0;
            //    }
            //    this.SliderWidthAndHeightLimit(item.RSCarouselSlider);
            //    this.SetCanvasLocation(item.RSCarouselSlider, RightDataList.Count, i, true, ref canvasLeft);
            //    this.CalcuTransform(item.RSCarouselSlider, RightDataList.Count, i, true, ref transformXTotal);
            //}

            //transformXTotal = 0;

            //for (int i = 0; i < LeftDataList.Count; i++)
            //{
            //    var item = LeftDataList[i];
            //    this.SliderWidthAndHeightLimit(item.RSCarouselSlider);
            //    this.SetCanvasLocation(item.RSCarouselSlider, LeftDataList.Count, i, false, ref canvasLeft);
            //    this.CalcuTransform(item.RSCarouselSlider, LeftDataList.Count, i, false, ref transformXTotal);
            //}

        }

        public double MaxScale = 1;
        public double MinScale = 0.2;


        /// <summary>
        /// 计算缩放比例
        /// </summary>
        private void CalcuTransform(RSCarouselSlider item, int count, int i, bool isRightSide, ref double transformXTotal)
        {
            //直接设置画布的总长度
            double drawMapWidth = 4000D;
            double marginRight = 30;
            double transformX = 0D;
            var halfDrawMapWidth = drawMapWidth / 2;

            var percent = (isRightSide ? i : i + 1) * this.SliderWidth / halfDrawMapWidth;
            var scale = MaxScale - percent;
            //缩放最小值0.2
            scale = Math.Max(MinScale, scale);
            var widthScale = item.Width * scale;
            var widthDif = item.Width - widthScale;

            if (isRightSide)
            {
                transformX = -widthDif / 2 - transformXTotal;
                if (i == 1)
                {
                    transformX = transformX + marginRight;
                }
                else if (i > 1)
                {
                    transformX = transformX - marginRight;
                }
            }
            else
            {
                transformX = widthDif / 2 + transformXTotal;

                if (i == 0)
                {
                    transformX = transformX - marginRight;
                }
                else
                {
                    transformX = transformX + marginRight;
                }
            }

            TransformGroup transformGroup = new TransformGroup();
            item.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform scaleTransform = new ScaleTransform()
            {
                ScaleX = scale,
                ScaleY = scale
            };
            transformGroup.Children.Add(scaleTransform);
            TranslateTransform translateTransform = new TranslateTransform()
            {
                X = transformX,
            };
            transformGroup.Children.Add(translateTransform);
            item.RenderTransform = transformGroup;

            transformXTotal += widthDif;
        }

        /// <summary>
        /// 轮廓图之间的高度差
        /// </summary>
        public int SliderHeightDif = 20;
        /// <summary>
        /// 计算每个控件在Canvas里左上角的坐标
        /// </summary>
        private void SetCanvasLocation(RSCarouselSlider item, int count, int i, bool isRightSide, ref double canvasLeft)
        {
            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;

            //canvasTop数值一致
            var canvasTop = canvasHeight / 2 - item.Height / 2;
            int ZIndex = 0;
            if (isRightSide)
            {
                ZIndex = count - i;

                if (i == 0)
                {
                    canvasLeft = canvasWidth / 2 - item.Width / 2;
                }
                else
                {
                    canvasLeft = canvasLeft + item.Width;
                }
            }
            else
            {
                ZIndex = count - i - 1;
                if (i == 0)
                {
                    canvasLeft = canvasWidth / 2 - item.Width / 2 - item.Width;
                }
                else
                {
                    canvasLeft = canvasLeft - item.Width;
                }
            }
            Canvas.SetLeft(item, canvasLeft);
            Canvas.SetTop(item, canvasTop);
            Panel.SetZIndex(item, ZIndex);
        }


        /// <summary>
        /// 重新计算Slider的宽和高
        /// </summary>
        /// <param name="item"></param>
        private void SliderWidthAndHeightLimit(RSCarouselSlider item)
        {
            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;
            //需要重新计算每一个Slider的默认长宽
            if (item.Height > canvasHeight)
            {
                item.Height = canvasHeight;
            }
            if (item.Width > canvasWidth)
            {
                item.Width = canvasWidth;
            }
        }


        private void RsCarouselSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var rsCarouselSlider = sender as RSCarouselSlider;

            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;
            //获取水平移动距离 这里可以直接通过水平移动值的正负判断是向左还是向右
            //负值向左，正值向右
            var horizontalChange = e.HorizontalChange;

            double drawMapWidth = 4000D;
            var halfDrawMapWidth = drawMapWidth / 2;
            var viewCenterX = canvasWidth / 2;
            var viewCenterY = canvasHeight / 2;


            var allDataList = RightDataList.Concat(LeftDataList).ToList();

            List<CarouselSlider> rightDataList = new List<CarouselSlider>();
            List<CarouselSlider> leftDataList = new List<CarouselSlider>();

            foreach (var item in allDataList)
            {
                var getLeft = Canvas.GetLeft(item.RSCarouselSlider);
                var getTop = Canvas.GetTop(item.RSCarouselSlider);
                getLeft += horizontalChange;
                //获取中心点
                var centerX = getLeft + item.RSCarouselSlider.Width / 2;
                var centerY = getTop + item.RSCarouselSlider.Height / 2;
                item.RSCarouselSlider.CenterPoint = new Point(centerX, centerY);
                var centerXDif = centerX - viewCenterX;
                if (centerXDif > 0)
                {
                    rightDataList.Add(item);
                }
                else
                {
                    leftDataList.Add(item);
                }
            }

            this.RightDataList = rightDataList.OrderBy(t => t.RSCarouselSlider.CenterPoint.X).ToList();
            this.LeftDataList = leftDataList.OrderByDescending(t => t.RSCarouselSlider.CenterPoint.X).ToList();

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Canvas = this.GetTemplateChild(nameof(PART_Canvas)) as Canvas;

        }
    }
}
