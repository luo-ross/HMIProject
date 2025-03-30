using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RS.Widgets.Controls
{

    /// <summary>
    ///  Fov算法，计算FieldOfView公式= 2 * arctan(尺寸 / (2 * 距离)) * (180 / π) 
    /// </summary>
    public class RSCarousel : RSUserControl
    {
        private Viewport3D PART_Viewport3D;
        private ModelVisual3D PART_ModelVisual3D;
        private double OffsetTotal { get; set; }
        private Point HistoryPosition { get; set; }
        private double Threhold = 15D;
        private bool IsCanMove { get; set; }

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
        public ObservableCollection<FrameworkElement> CarouselSliderList
        {
            get { return (ObservableCollection<FrameworkElement>)GetValue(CarouselSliderListProperty); }
            set { SetValue(CarouselSliderListProperty, value); }
        }

        public static readonly DependencyProperty CarouselSliderListProperty =
            DependencyProperty.Register("CarouselSliderList", typeof(ObservableCollection<FrameworkElement>), typeof(RSCarousel), new FrameworkPropertyMetadata(null, CarouselSliderListPropertyChanged));

        private static void CarouselSliderListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsCarousel = d as RSCarousel;
            rsCarousel.InitCarouselSlider(rsCarousel.CarouselSliderList.Count / 2);
        }


        [Description("相机X轴偏移位置")]
        public double OffsetX
        {
            get { return (double)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); }
        }

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register("OffsetX", typeof(double), typeof(RSCarousel), new PropertyMetadata(0D));



        [Description("X轴轮播图像之间的距离")]
        public double AxisXDistance
        {
            get { return (double)GetValue(AxisXDistanceProperty); }
            set { SetValue(AxisXDistanceProperty, value); }
        }

        public static readonly DependencyProperty AxisXDistanceProperty =
            DependencyProperty.Register("AxisXDistance", typeof(double), typeof(RSCarousel), new PropertyMetadata(60D));



        [Description("Z轴上轮播图片之间的距离")]
        public double OffsetZ
        {
            get { return (double)GetValue(OffsetZProperty); }
            set { SetValue(OffsetZProperty, value); }
        }

        public static readonly DependencyProperty OffsetZProperty =
            DependencyProperty.Register("OffsetZ", typeof(double), typeof(RSCarousel), new PropertyMetadata(180D));



        [Description("镜头位置")]
        public Point3D CameraPosition
        {
            get { return (Point3D)GetValue(CameraPositionProperty); }
            set { SetValue(CameraPositionProperty, value); }
        }

        public static readonly DependencyProperty CameraPositionProperty =
            DependencyProperty.Register("CameraPosition", typeof(Point3D), typeof(RSCarousel), new PropertyMetadata(new Point3D(0, 0, 700D)));









        private List<TranslateTransform3D> TranslateTransform3DList = new List<TranslateTransform3D>();
        private void InitCarouselSlider(int middleIndex)
        {
            if (this.PART_ModelVisual3D == null)
            {
                return;
            }
            this.PART_ModelVisual3D.Children.Clear();

            for (int i = 0; i < this.CarouselSliderList.Count; i++)
            {
                var item = this.CarouselSliderList[i];
                item.Width = this.SliderWidth;
                item.Height = this.SliderHeight;
                Viewport2DVisual3D viewport2DVisual3D = new Viewport2DVisual3D();
                // 创建长方形的几何模型
                MeshGeometry3D rectangleGeometry = new MeshGeometry3D();
                rectangleGeometry.Positions = new Point3DCollection()
                {
                new Point3D(-this.SliderWidth / 2, -this.SliderHeight / 2, 0),
                new Point3D(this.SliderWidth / 2, -this.SliderHeight / 2, 0),
                new Point3D(this.SliderWidth / 2, this.SliderHeight / 2, 0),
                new Point3D(-this.SliderWidth / 2, this.SliderHeight / 2, 0)
                };
                rectangleGeometry.TriangleIndices = new Int32Collection() { 0, 1, 2, 2, 3, 0 };
                rectangleGeometry.TextureCoordinates = new PointCollection();
                rectangleGeometry.TextureCoordinates.Add(new System.Windows.Point(0, 1));
                rectangleGeometry.TextureCoordinates.Add(new System.Windows.Point(1, 1));
                rectangleGeometry.TextureCoordinates.Add(new System.Windows.Point(1, 0));
                rectangleGeometry.TextureCoordinates.Add(new System.Windows.Point(0, 0));

                // 创建 DiffuseMaterial 并设置属性
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
                diffuseMaterial.Brush = System.Windows.Media.Brushes.Blue;
                Viewport2DVisual3D.SetIsVisualHostMaterial(diffuseMaterial, true);
                viewport2DVisual3D.Geometry = rectangleGeometry;
                viewport2DVisual3D.Material = diffuseMaterial;
                viewport2DVisual3D.Visual = item;
                item.MouseLeftButtonDown += Item_MouseLeftButtonDown;
                item.MouseLeftButtonUp += Item_MouseLeftButtonUp;
                Transform3DGroup transform3DGroup = new Transform3DGroup();
                TranslateTransform3D translateTransform3D = new TranslateTransform3D()
                {
                    OffsetX = (this.SliderWidth + this.AxisXDistance) * (i - middleIndex),
                    OffsetY = 0,
                };
                viewport2DVisual3D.Transform = transform3DGroup;
                transform3DGroup.Children.Add(translateTransform3D);
                TranslateTransform3DList.Add(translateTransform3D);
                this.PART_ModelVisual3D.Children.Add(viewport2DVisual3D);
            }

            RefreshPosition();
        }

        private void Item_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsCanMove = false;
        }


        private void Item_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsCanMove = true;
            HistoryPosition = e.GetPosition(this.PART_Viewport3D);
        }


        private void RefreshPosition()
        {
            foreach (var item in this.TranslateTransform3DList)
            {
                // 遍历Transform3DGroup中的所有变换
                var distance = item.OffsetX - this.OffsetX;
                double xPeriod = Math.Abs(distance) / (this.SliderWidth + this.AxisXDistance);
                double xInPeriod = Math.Abs(distance) % (this.SliderWidth + this.AxisXDistance);
                double offsetZ = -(xInPeriod / (this.SliderWidth + this.AxisXDistance) * this.OffsetZ + xPeriod * this.OffsetZ);
                item.OffsetZ = offsetZ;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Viewport3D = this.GetTemplateChild(nameof(PART_Viewport3D)) as Viewport3D;
            this.PART_ModelVisual3D = this.GetTemplateChild(nameof(PART_ModelVisual3D)) as ModelVisual3D;

            if (this.PART_Viewport3D != null)
            {
                this.PART_Viewport3D.PreviewMouseMove += PART_Viewport3D_PreviewMouseMove;
                this.InitCarouselSlider(this.CarouselSliderList.Count / 2);
            }
        }

        private void PART_Viewport3D_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this.PART_Viewport3D);

                Threhold = Math.Abs(currentPosition.X - HistoryPosition.X);
                if (currentPosition.X > HistoryPosition.X)
                {
                    this.OffsetX = this.OffsetX - Threhold;
                    this.OffsetTotal -= Threhold;
                }
                else
                {
                    this.OffsetX = this.OffsetX + Threhold;
                    this.OffsetTotal += Threhold;
                }


                RefreshPosition();

                var minOffsetX = this.TranslateTransform3DList.Min(t => t.OffsetX);
                var maxOffsetX = this.TranslateTransform3DList.Max(t => t.OffsetX);

                var first = this.TranslateTransform3DList.FirstOrDefault(t => t.OffsetX == minOffsetX);
                var last = this.TranslateTransform3DList.FirstOrDefault(t => t.OffsetX == maxOffsetX);

                if (Math.Abs(this.OffsetTotal) >= this.SliderWidth + this.AxisXDistance)
                {
                    if (currentPosition.X > HistoryPosition.X)
                    {
                        last.OffsetX = first.OffsetX - (this.SliderWidth + this.AxisXDistance);
                        last.OffsetZ = first.OffsetZ - this.OffsetZ;
                    }
                    else
                    {
                        first.OffsetX = last.OffsetX + (this.SliderWidth + this.AxisXDistance);
                        first.OffsetZ = last.OffsetZ - this.OffsetZ;
                    }
                    this.OffsetTotal = 0;
                }
                HistoryPosition = currentPosition;
            }
        }
    }
}
