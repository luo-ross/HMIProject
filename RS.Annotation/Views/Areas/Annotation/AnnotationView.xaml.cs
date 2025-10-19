using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp.WpfExtensions;
using RS.Commons;
using RS.Commons.Enums;
using RS.Commons.Helper;
using RS.Widgets.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using RS.Annotation.Commons;
using RS.Annotation.Enums;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Shapes;
using RS.Widgets.Controls;

namespace RS.Annotation.Views.Areas
{
    /// <summary>
    /// 目标检测标注视图
    /// </summary>
    public partial class AnnotationView : RSDialog
    {
        /// <summary>
        /// 当前视图MVVM 绑定的ViewModel
        /// </summary>
        public AnnotationViewModel ViewModel { get; set; }


        /// <summary>
        /// 自定义画笔
        /// </summary>
        public Cursor DrawCursor { get; set; }

        /// <summary>
        /// 小手鼠标
        /// </summary>
        public Cursor HandCursor { get; set; }

        /// <summary>
        /// 拖拽 拳头鼠标
        /// </summary>
        public Cursor DragCursor { get; set; }

        /// <summary>
        /// 矩形最小宽度
        /// </summary>
        public double MinRectWidth = 1;

        /// <summary>
        /// 矩形最小高度
        /// </summary>
        public double MinRectHeight = 1;

        /// <summary>
        /// 历史矩形选择
        /// </summary>
        public List<RectModel> SelectRectModelHistoryList = new List<RectModel>();

        /// <summary>
        /// 标注原始图像
        /// </summary>
        public System.Drawing.Bitmap ImgBitmap { get; set; }

        /// <summary>
        /// 获取标注Canvas容器鼠标按下去时的坐标
        /// </summary>
        public Point AnnotationCanvasHostMouseDownPosition { get; set; }

        /// <summary>
        /// 获取标注Canvas鼠标按下去时的坐标
        /// </summary>
        public Point AnnotationCanvasMouseDownPosition { get; set; }

        /// <summary>
        /// 获取标注Canvas容器鼠标按下去时记录的历史坐标
        /// </summary>
        public Cursor AnnotationCanvasHost_MouseDownCursorHistory { get; set; }

        /// <summary>
        /// 记录标注Canvas容器坐标左键是否按下
        /// </summary>
        private bool IsAnnotationCanvasHostMouseLeftPressDown = false;

        /// <summary>
        /// 用这个标志位来记录当前事件是否是标注视窗上的鼠标平移事件
        /// </summary>
        public bool IsAnnotationCanvasHostMouseMoveEvent { get; set; }

        /// <summary>
        /// 标注矩形多选框绘制
        /// </summary>
        public RectModel MultiSelectRectModelDraw { get; set; }

        /// <summary>
        /// 存储矩形框Resize历史数据 Key:矩形框 Value:元组数据记录矩形框的历史坐上角位置X,Y和长宽数据
        /// </summary>
        public Dictionary<RectModel, (double CanvasLeft, double CanvasTop, double Width, double Height)> ResizeRectDataDic = new Dictionary<RectModel, (double CanvasLeft, double CanvasTop, double Width, double Height)>();

        /// <summary>
        /// 绘制一个新的矩形框
        /// </summary>
        public RectModel RectModelDraw { get; set; }

        /// <summary>
        /// 标注矩形框尺寸更改事件类型 
        /// </summary>
        public RectResizeEvnets RectResizeEvnets { get; set; }

        /// <summary>
        /// 记录Ctrl键按下时鼠标的样式
        /// </summary>
        public Cursor CtrlKeyDownCursorHistory { get; set; }

        /// <summary>
        /// 记录AltKeyDown鼠标样式
        /// </summary>
        public Cursor AltKeyDownCursorHistory { get; set; }

        /// <summary>
        /// 记录ShiftKeyDown鼠标样式
        /// </summary>
        public Cursor ShiftDownCursorHistory { get; set; }

        /// <summary>
        /// 标注矩形框复制列表
        /// </summary>
        private List<RectModel> RectModelCopyList = new List<RectModel>();

        /// <summary>
        /// 标注类别增改视图
        /// </summary>
        public TagEditPopView TagEditPopView { get; set; }

        /// <summary>
        /// 是否是导航器的缩放
        /// </summary>
        public bool IsNavScale = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AnnotationView()
        {
            InitializeComponent();

            //加载绘制画笔鼠标样式
            StreamResourceInfo drawCur = Application.GetResourceStream(new Uri("pack://application:,,,/RS.Annotation;component/Assets/BlueDraw.cur", UriKind.RelativeOrAbsolute));
            this.DrawCursor = new Cursor(drawCur.Stream);

            //加载拖拽鼠标样式
            StreamResourceInfo dragCur = Application.GetResourceStream(new Uri("pack://application:,,,/RS.Annotation;component/Assets/BlueDrag.cur", UriKind.RelativeOrAbsolute));
            this.DragCursor = (Cursor)CursorHelper.InternalCreateCursor((System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(dragCur.Stream), 0, 0);

            //加载暂停工作鼠标式样
            StreamResourceInfo handCur = Application.GetResourceStream(new Uri("pack://application:,,,/RS.Annotation;component/Assets/BlueHand.cur", UriKind.RelativeOrAbsolute));
            this.HandCursor = (Cursor)CursorHelper.InternalCreateCursor((System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(handCur.Stream), 0, 0);

            //设置默认鼠标样式
            this.Cursor = Cursors.Arrow;

            //获取当前空间MVVM ViewModel数据
            this.ViewModel = this.DataContext as AnnotationViewModel;

            //控件加载完成事件
            this.Loaded += AnnotationView_Loaded;

            //控件尺寸改变之后触发的事件
            this.SizeChanged += AnnotationView_SizeChanged;
        }

        /// <summary>
        /// 标注窗体尺寸发生变化触发事件
        /// </summary>
        private void AnnotationView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }
            this.LoadImgModelData();
        }

        /// <summary>
        /// 标注控件加载完成事件
        /// </summary>
        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {

        }



        [Description("项目标注视图")]
        [Browsable(true)]
        public ProjectsView ProjectsView
        {
            get { return (ProjectsView)GetValue(ProjectsViewProperty); }
            set { SetValue(ProjectsViewProperty, value); }
        }

        public static readonly DependencyProperty ProjectsViewProperty =
            DependencyProperty.Register("ProjectsView", typeof(ProjectsView), typeof(AnnotationView), new PropertyMetadata(default));


        [Description("项目图像数据集视图")]
        [Browsable(true)]
        public PicturesView PicturesView
        {
            get { return (PicturesView)GetValue(PicturesViewProperty); }
            set { SetValue(PicturesViewProperty, value); }
        }

        public static readonly DependencyProperty PicturesViewProperty =
            DependencyProperty.Register("PicturesView", typeof(PicturesView), typeof(AnnotationView), new PropertyMetadata(default));



        [Description("用户选择要标注的图像")]
        [Browsable(true)]
        public ImgModel ImgModelSelect
        {
            get { return (ImgModel)GetValue(ImgModelSelectProperty); }
            set { SetValue(ImgModelSelectProperty, value); }
        }
        public static readonly DependencyProperty ImgModelSelectProperty =
            DependencyProperty.Register("ImgModelSelect", typeof(ImgModel), typeof(AnnotationView), new PropertyMetadata(default, OnImgModelSelectPropertyChanged));

        /// <summary>
        /// 用户标注图像选择发生变化后触发事件
        /// </summary>
        private static void OnImgModelSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var annotationView = d as AnnotationView;
            annotationView.LoadImgModelData();
        }

        /// <summary>
        /// 加载标注图像数据
        /// </summary>
        private void LoadImgModelData()
        {
            this.AnnotationCanvas.Focus();
            this.ViewModel.RectModelSelect = null;
            this.ViewModel.TagModelSelect = null;
            this.ViewModel.ImgSource = null;
            this.AnnotationCanvas.Children.Clear();

            var imgModel = this.ImgModelSelect;
            if (imgModel == null)
            {
                return;
            }

            try
            {
                //这里加载图象时会出现4通道的图像 会导致无法显示
                this.ViewModel.ImgSource = ImgHelper.GetBitmapSource(imgModel.ImgPath);
                this.ImgBitmap = this.ViewModel.ImgSource.ToBitmap();

            }
            catch (Exception ex)
            {
                this.ImgBitmap = null;
                this.Cursor = Cursors.Arrow;
                return;
            }


            foreach (var item in imgModel.RectModelList)
            {
                this.AddRectModel(item);
            }
            SizeAutoFit(this.ViewModel.ImgSource);

            this.RefreshRectModelStyle();
        }

        /// <summary>
        /// 标注图像显示自动拟合
        /// </summary>
        /// <param name="imageSource">标注图像资源</param>
        public void SizeAutoFit(ImageSource imageSource)
        {
            //获取图像的尺寸
            var imgWidth = imageSource.Width;
            var imgheight = imageSource.Height;

            //获取标注视窗的实际尺寸
            var actualWidth = this.AnnotationCanvasHost.ActualWidth;
            var actualHeight = this.AnnotationCanvasHost.ActualHeight;

            //获取视窗宽高的一半尺寸 也就是视窗的中心
            var halfW = actualWidth / 2;
            var halfH = actualHeight / 2;

            //如果图像的长款已经小雨视窗的2分之一 就不能在继续缩小了
            if (imgWidth < halfW && imgheight < halfH)
            {
                this.ViewModel.MinScale = 1;
            }
            else
            {
                var widthRatio = imgWidth / halfW;
                var heightRatio = imgheight / halfH;
                this.ViewModel.MinScale = 1D / Math.Min(widthRatio, heightRatio);
            }

            //计算按照长度还是宽度来进行比例缩放
            var scale1 = actualWidth / imgWidth;
            var scale2 = actualHeight / imgheight;
            this.ViewModel.Scale = Math.Min(scale1, scale2);

            this.ViewModel.ScaleX = this.ViewModel.Scale;
            this.ViewModel.ScaleY = this.ViewModel.Scale;

            //我们以图像正中心进行缩放
            var imgCenterX = imgWidth / 2;
            var imgCenterY = imgheight / 2;

            this.ViewModel.CenterX = imgCenterX;
            this.ViewModel.CenterY = imgCenterY;

            //获取视窗的正中心 并设置平移
            this.ViewModel.TransformX = halfW - imgCenterX;
            this.ViewModel.TransformY = halfH - imgCenterY;
        }

        /// <summary>
        /// 标注图像自动拟合点击事件
        /// </summary>
        private void BtnSizeAuto_Click(object sender, RoutedEventArgs e)
        {
            this.SizeAutoFit(this.ViewModel.ImgSource);
        }

        /// <summary>
        /// 查看下一个图像点击事件
        /// </summary>
        private void BtnNextImgModel_Click(object sender, RoutedEventArgs e)
        {
            this.PicturesView.SetImgModelSelectWithArrowKey(Key.Right);
        }

        /// <summary>
        /// 查看上一个图像点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreImgModel_Click(object sender, RoutedEventArgs e)
        {
            this.PicturesView.SetImgModelSelectWithArrowKey(Key.Left);
        }

        /// <summary>
        /// 删除所选图像事件
        /// </summary>
        private void BtnDeleteImgSelect_Click(object sender, RoutedEventArgs e)
        {
            this.PicturesView.DeleteImgSelect();
        }

        /// <summary>
        /// 标注Canvas鼠标滚轮事件
        /// </summary>
        private void AnnotationCanvasHost_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                this.IsNavScale = false;
                this.UpdateScale(e.Delta > 0);

                var zoomCenter = e.GetPosition(ImgCanvas);

                this.UpdateAnnotaionCanvasScale(zoomCenter);

                this.UpdateNavRectModelStyle();

                this.RefreshRectModelStyle();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsNavScale = true;
            }
        }

        /// <summary>
        /// 更新标注Canvas缩放
        /// </summary>
        /// <param name="zoomCenter">标注缩放中心坐标</param>
        private void UpdateAnnotaionCanvasScale(Point zoomCenter)
        {
            if (this.ViewModel == null)
            {
                return;
            }
            var pointToContent = this.ImgCanvas.RenderTransform.Inverse.Transform(zoomCenter);

            this.ViewModel.TransformX = (zoomCenter.X - pointToContent.X) * this.ViewModel.ScaleX;
            this.ViewModel.TransformY = (zoomCenter.Y - pointToContent.Y) * this.ViewModel.ScaleY;

            this.ViewModel.CenterX = zoomCenter.X;
            this.ViewModel.CenterY = zoomCenter.Y;
            this.ViewModel.ScaleX = this.ViewModel.Scale;
            this.ViewModel.ScaleY = this.ViewModel.Scale;
        }

        /// <summary>
        /// 更新标注视窗缩放值
        /// </summary>
        /// <param name="isScaleUp">True放大 False缩小</param>
        private void UpdateScale(bool isScaleUp)
        {
            var scale = isScaleUp ? this.ViewModel.Scale + 1 : this.ViewModel.Scale - 1;
            scale = Math.Max(this.ViewModel.MinScale, scale);
            scale = Math.Min(this.ViewModel.MaxScale, scale);
            this.ViewModel.Scale = scale;
        }

        private bool IsAnnotationCanvasHost_MouseDown;
        /// <summary>
        /// 标注Canvas容器鼠标按下事件
        /// </summary>
        private void AnnotationCanvasHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsAnnotationCanvasHost_MouseDown = true;
            //这是标注视窗按下的位置
            this.AnnotationCanvasHostMouseDownPosition = e.GetPosition(this.AnnotationCanvasHost);

            //这是标注Canvas按下的点 这2个点不一样
            this.AnnotationCanvasMouseDownPosition = e.GetPosition(this.AnnotationCanvas);

            //记录当前鼠标按下时的平移变换位置
            this.ViewModel.CanvasMouseDownTranslateTransformPosition = new Point(this.ViewModel.TransformX, this.ViewModel.TransformY);

            //这里是改变鼠标样式 以便拖拽
            if (e.ChangedButton == MouseButton.Middle)
            {
                this.Cursor = this.DragCursor;
            }

            //这是记录上一次鼠标样式 以便还原
            if (AnnotationCanvasHost_MouseDownCursorHistory == null)
            {
                AnnotationCanvasHost_MouseDownCursorHistory = this.Cursor;
            }

            this.IsAnnotationCanvasHostMouseLeftPressDown = true;
            e.Handled = true;
        }

        /// <summary>
        /// 标注Canvas容易鼠标移动事件
        /// </summary>
        private void AnnotationCanvasHost_MouseMove(object sender, MouseEventArgs e)
        {
            //这个是获取视窗上平移的点
            this.ViewModel.AnnotationCanvasHostMouseMovePosition = e.GetPosition(this.AnnotationCanvasHost);
            //这个是获取标注Canvas上平移的点
            this.ViewModel.AnnotationCanvasMouseMovePosition = e.GetPosition(this.AnnotationCanvas);

            //这里是配合鼠标样式做事件判断 也就是使用鼠标样式来作为事件驱动

            IsAnnotationCanvasHostMouseMoveEvent = true;
            //这里触发图像拖拽
            if ((e.MiddleButton == MouseButtonState.Pressed && this.Cursor == this.DragCursor)
                || (e.LeftButton == MouseButtonState.Pressed && this.Cursor == this.DragCursor))
            {
                this.ViewDragEvent();
            }
            //这里触发绘制矩形
            else if (this.Cursor == this.DrawCursor && e.LeftButton == MouseButtonState.Pressed && IsAnnotationCanvasHostMouseLeftPressDown)
            {
                this.DrawRectangleEvent();
            }
            else if (this.Cursor == Cursors.Cross && e.LeftButton == MouseButtonState.Pressed)
            {
                //保证鼠标移动的时候获取到第一个点
                if (this.AnnotationCanvasHostMouseDownPosition == default)
                {
                    this.AnnotationCanvasHostMouseDownPosition = this.ViewModel.AnnotationCanvasMouseMovePosition;
                }

                //多选事件
                this.DrawMultiSelectRetangleEvent();

            }
            else if (e.LeftButton == MouseButtonState.Pressed
                && (this.Cursor == Cursors.SizeNWSE
                || this.Cursor == Cursors.SizeNESW
                || this.Cursor == Cursors.SizeNS
                || this.Cursor == Cursors.SizeWE
                 || this.Cursor == Cursors.SizeAll
                ))
            {
                this.RectResizeEvent(this.RectResizeEvnets);
            }
            else
            {
                IsAnnotationCanvasHostMouseMoveEvent = false;
                this.RefreshCursorStyle();
            }

        }

        /// <summary>
        /// 绘制标注多选矩形框事件
        /// </summary>
        private void DrawMultiSelectRetangleEvent()
        {
            //获取矩形框在Canvas左上角位置
            double canvasLeft = this.AnnotationCanvasMouseDownPosition.X;
            double canvasTop = this.AnnotationCanvasMouseDownPosition.Y;
            //获取移动距离
            (double xDistance, double yDistance) = this.GetMoveDistance(this.ViewModel.AnnotationCanvasMouseMovePosition, this.AnnotationCanvasMouseDownPosition);
            if (xDistance < 0)
            {
                canvasLeft = this.ViewModel.AnnotationCanvasMouseMovePosition.X;
            }
            if (yDistance < 0)
            {
                canvasTop = this.ViewModel.AnnotationCanvasMouseMovePosition.Y;
            }
            var xDistanceAbs = Math.Abs(xDistance);
            var yDistanceAbs = Math.Abs(yDistance);

            if (xDistanceAbs < this.MinRectWidth || yDistanceAbs < this.MinRectHeight)
            {
                return;
            }

            if (this.MultiSelectRectModelDraw == null)
            {
                //清除选择
                this.ClearAllRectModelSelect();

                //刷新矩形边框
                this.RefreshRectModelStyle();

                string tagColor = "#1296db";
                this.MultiSelectRectModelDraw = new RectModel(Guid.NewGuid().ToString(),this.ImgModelSelect.Id, this.ProjectsView.ViewModel.ProjectModelSelect.Id)
                {
                    Width = xDistanceAbs,
                    Height = yDistanceAbs,
                    CanvasLeft = canvasLeft,
                    CanvasTop = canvasTop,
                    TagModel = new TagModel(Guid.NewGuid().ToString(), this.ProjectsView.ViewModel.ProjectModelSelect.Id)
                    {
                        TagColor = tagColor
                    },
                    Rectangle = new Rectangle()
                    {
                        StrokeThickness = 2 / this.ViewModel.Scale,
                        Fill = GetFillColor(tagColor),
                        StrokeDashArray = new DoubleCollection() { 1, 1 }
                    }
                };

                this.SetRectModelBinding(this.MultiSelectRectModelDraw);

                if (!this.AnnotationCanvas.Children.Contains(this.MultiSelectRectModelDraw.Rectangle))
                {
                    this.AnnotationCanvas.Children.Add(this.MultiSelectRectModelDraw.Rectangle);
                }
            }
            else
            {
                this.MultiSelectRectModelDraw.Width = xDistanceAbs;
                this.MultiSelectRectModelDraw.Height = yDistanceAbs;
                this.MultiSelectRectModelDraw.CanvasLeft = canvasLeft;
                this.MultiSelectRectModelDraw.CanvasTop = canvasTop;
                this.MultiSelectRectModelDraw.Rectangle.StrokeThickness = 2 / this.ViewModel.Scale;
                this.MultiRectSelectEvent();
            }
        }

        /// <summary>
        /// 标注矩形框多选事件
        /// </summary>
        private void MultiRectSelectEvent()
        {
            if (this.MultiSelectRectModelDraw != null)
            {
                var width = this.MultiSelectRectModelDraw.Width;
                var height = this.MultiSelectRectModelDraw.Height;
                var canvasLeft = this.MultiSelectRectModelDraw.CanvasLeft;
                var canvasTop = this.MultiSelectRectModelDraw.CanvasTop;

                var rectModelList = this.PicturesView.ViewModel.ImgModelSelect.RectModelList;

                foreach (var rectModel in rectModelList)
                {
                    var rectWidth = rectModel.Width;
                    var rectHeight = rectModel.Height;
                    var rectLeft = rectModel.CanvasLeft;
                    var rectTop = rectModel.CanvasTop;

                    if (rectLeft >= canvasLeft
                        && rectTop >= canvasTop
                        && rectWidth + rectLeft <= width + canvasLeft
                        && rectHeight + rectTop <= height + canvasTop)
                    {
                        rectModel.IsSelect = true;
                    }
                    else
                    {
                        if (rectModel.IsSelect)
                        {
                            rectModel.IsSelect = false;
                        }
                    }
                }
            }

            this.RefreshRectModelStyle();
        }


        /// <summary>
        /// 标注矩形框数据更新事件
        /// </summary>
        /// <param name="rectResizeEvnets">尺寸更新事件类型</param>
        private void RectResizeEvent(RectResizeEvnets rectResizeEvnets)
        {
            var rectModelList = this.GetRectModelSelectList();
            if (rectModelList.Count == 0)
            {
                return;
            }

            //获取移动距离
            (double xDistance, double yDistance) = GetMoveDistance(this.ViewModel.AnnotationCanvasMouseMovePosition, this.AnnotationCanvasMouseDownPosition);

            //这里就是批量对我们选中的矩形框进行尺寸调整
            foreach (var rectModel in rectModelList)
            {
                if (!this.ResizeRectDataDic.ContainsKey(rectModel))
                {
                    this.ResizeRectDataDic[rectModel] = (rectModel.CanvasLeft, rectModel.CanvasTop, rectModel.Width, rectModel.Height);
                    continue;
                }

                double width = this.ResizeRectDataDic[rectModel].Width;
                double height = this.ResizeRectDataDic[rectModel].Height;

                switch (rectResizeEvnets)
                {
                    case RectResizeEvnets.ResizeLeftBorder:
                        this.ResizeLeftBorderEvent(rectModel, width, xDistance);
                        break;
                    case RectResizeEvnets.ResizeTopBorder:
                        this.ResizeTopBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeRightBorder:
                        this.ResizeRightBorderEvent(rectModel, width, xDistance);
                        break;
                    case RectResizeEvnets.ResizeBottomBorder:
                        this.ResizeBottomBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeLeftTopCorner:
                        this.ResizeLeftBorderEvent(rectModel, width, xDistance);
                        this.ResizeTopBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeRightTopCorner:
                        this.ResizeRightBorderEvent(rectModel, width, xDistance);
                        this.ResizeTopBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeRightBottomCorner:
                        this.ResizeRightBorderEvent(rectModel, width, xDistance);
                        this.ResizeBottomBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeLeftBottomCorner:
                        this.ResizeLeftBorderEvent(rectModel, width, xDistance);
                        this.ResizeBottomBorderEvent(rectModel, height, yDistance);
                        break;
                    case RectResizeEvnets.ResizeAll:
                        var canvasLeft = this.ResizeRectDataDic[rectModel].CanvasLeft + xDistance;
                        var canvasTop = this.ResizeRectDataDic[rectModel].CanvasTop + yDistance;
                        rectModel.CanvasLeft = canvasLeft;
                        rectModel.CanvasTop = canvasTop;
                        break;
                }

            }
        }

        /// <summary>
        /// 矩形框下边框更改事件
        /// </summary>
        /// <param name="rectModel">目标矩形框</param>
        /// <param name="height">矩形框历史高度</param>
        /// <param name="yDistance">鼠标按下去时坐标和鼠标移动后坐标之间的Y水平方向距离</param>
        private void ResizeBottomBorderEvent(RectModel rectModel, double height, double yDistance)
        {
            if (yDistance < this.MinRectHeight - height)
            {
                yDistance = this.MinRectHeight - height;
            }
            height = height + yDistance;
            rectModel.Height = height;
        }

        /// <summary>
        /// 矩形框右边框更改事件
        /// </summary>
        /// <param name="rectModel">目标矩形框</param>
        /// <param name="width">矩形框历史宽度</param>
        /// <param name="xDistance">鼠标按下去时坐标和鼠标移动后坐标之间的X垂直方向距离</param>
        private void ResizeRightBorderEvent(RectModel rectModel, double width, double xDistance)
        {
            if (xDistance < this.MinRectWidth - width)
            {
                xDistance = this.MinRectWidth - width;
            }
            width = width + xDistance;
            rectModel.Width = width;
        }

        /// <summary>
        /// 矩形框上边框更改事件
        /// </summary>
        /// <param name="rectModel">目标矩形框</param>
        /// <param name="height">矩形框历史高度</param>
        /// <param name="yDistance">鼠标按下去时坐标和鼠标移动后坐标之间的Y水平方向距离</param>
        private void ResizeTopBorderEvent(RectModel rectModel, double height, double yDistance)
        {
            if (yDistance > height - this.MinRectHeight)
            {
                yDistance = height - this.MinRectHeight;
            }
            height = height - yDistance;
            rectModel.Height = height;
            var canvasTop = this.ResizeRectDataDic[rectModel].CanvasTop + yDistance;
            rectModel.CanvasTop = canvasTop;
        }


        /// <summary>
        /// 矩形框左边框更改事件
        /// </summary>
        /// <param name="rectModel">目标矩形框</param>
        /// <param name="width">矩形框历史宽度</param>
        /// <param name="xDistance">鼠标按下去时坐标和鼠标移动后坐标之间的X垂直方向距离</param>
        private void ResizeLeftBorderEvent(RectModel rectModel, double width, double xDistance)
        {
            //我们必须保证我们的宽度不能重合 至少保证 宽度和高度不能小于最小值
            if (xDistance > width - this.MinRectWidth)
            {
                xDistance = width - this.MinRectWidth;
            }
            width = width - xDistance;
            rectModel.Width = width;
            var canvasLeft = this.ResizeRectDataDic[rectModel].CanvasLeft + xDistance;
            rectModel.CanvasLeft = canvasLeft;
        }


        /// <summary>
        /// 获取鼠标按下后和鼠标移动后2个点之间的X，Y方向的距离
        /// </summary>
        /// <param name="mouseMovePoint">鼠标按下时的坐标</param>
        /// <param name="mouseDownPostion">鼠标移动后的坐标</param>
        /// <returns></returns>
        public (double xDistance, double yDistance) GetMoveDistance(Point mouseMovePoint, Point mouseDownPostion)
        {
            var xDistance = mouseMovePoint.X - mouseDownPostion.X;
            var yDistance = mouseMovePoint.Y - mouseDownPostion.Y;
            return (xDistance, yDistance);
        }

        /// <summary>
        /// 绘制一个新的标注矩形框事件
        /// </summary>
        private void DrawRectangleEvent()
        {
            //获取这个项目的标签列表
            var tagModleList = this.GetTagModelList();

            //假如用户主动选择 就用用户选择的
            if (this.ViewModel.TagModelSelect == null)
            {
                this.ViewModel.TagModelSelect = tagModleList.FirstOrDefault(t => t.IsSelect);
            }

            //假如用户没选择 就用默认第一个作为标注
            if (this.ViewModel.TagModelSelect == null)
            {
                this.ViewModel.TagModelSelect = tagModleList.FirstOrDefault();
            }

            //假如这里还是空 那么我们就需要提示让用户去添加一个标签实例
            if (this.ViewModel.TagModelSelect == null)
            {
                //这个业务待实现
                var tagModel = this.InitTagModel();
                tagModel.ClassName = "类别1";
                tagModel.IsSelect = true;
                this.ViewModel.TagModelSelect = tagModel;
                this.AddTagModel(tagModel);
            }

            //获取矩形的左上角所在位置
            double canvasLeft = this.AnnotationCanvasMouseDownPosition.X;
            double canvasTop = this.AnnotationCanvasMouseDownPosition.Y;

            //使用元组数据返回
            (double xDistance, double yDistance) = GetMoveDistance(this.ViewModel.AnnotationCanvasMouseMovePosition, this.AnnotationCanvasMouseDownPosition);
            if (xDistance < 0)
            {
                canvasLeft = this.ViewModel.AnnotationCanvasMouseMovePosition.X;
            }
            if (yDistance < 0)
            {
                canvasTop = this.ViewModel.AnnotationCanvasMouseMovePosition.Y;
            }

            var xDistanceAbs = Math.Abs(xDistance);
            var yDistanceAbs = Math.Abs(yDistance);

            //保证我们矩形最小宽度和高度
            if (xDistanceAbs < this.MinRectWidth || yDistanceAbs < this.MinRectHeight)
            {
                return;
            }

            if (this.RectModelDraw == null)
            {
                this.ClearAllRectModelSelect();

                this.RectModelDraw = new RectModel(Guid.NewGuid().ToString(), this.ImgModelSelect.Id, this.ProjectsView.ViewModel.ProjectModelSelect.Id)
                {
                    Width = xDistanceAbs,
                    Height = yDistanceAbs,
                    CanvasLeft = canvasLeft,
                    CanvasTop = canvasTop,
                    TagModel = this.ViewModel.TagModelSelect,
                    Rectangle = new Rectangle()
                    {
                        Fill = GetFillColor(this.ViewModel.TagModelSelect.TagColor),
                        StrokeThickness = 2 / this.ViewModel.Scale
                    }
                };
                this.SetRectModelBinding(this.RectModelDraw);
                this.AddRectModel(this.RectModelDraw);
            }
            else
            {
                this.RectModelDraw.Width = xDistanceAbs;
                this.RectModelDraw.Height = yDistanceAbs;
                this.RectModelDraw.CanvasLeft = canvasLeft;
                this.RectModelDraw.CanvasTop = canvasTop;
                this.RectModelDraw.Rectangle.StrokeThickness = 2 / this.ViewModel.Scale;
            }

        }

        /// <summary>
        /// 添加项目标注类别
        /// </summary>
        /// <param name="tagModel">标注类别</param>
        private void AddTagModel(TagModel tagModel)
        {
            this.ProjectsView.ViewModel.ProjectModelSelect.TagModelList.Add(tagModel);
        }

        /// <summary>
        /// 清除所有标注矩形框选择
        /// </summary>
        private void ClearAllRectModelSelect()
        {
            foreach (var rectModel in PicturesView.ViewModel.ImgModelSelect.RectModelList)
            {
                rectModel.IsSelect = false;
            }
            this.ViewModel.RectModelSelect = null;
        }

        /// <summary>
        /// 绑定标注矩形框依赖数据 MVVM数据后台编码绑定
        /// </summary>
        /// <param name="rectModel"></param>
        private void SetRectModelBinding(RectModel rectModel)
        {
            rectModel.Rectangle.DataContext = rectModel;
            rectModel.Rectangle.SetBinding(WidthProperty, nameof(RectModel.Width));
            rectModel.Rectangle.SetBinding(HeightProperty, nameof(RectModel.Height));
            rectModel.Rectangle.SetBinding(Canvas.LeftProperty, nameof(RectModel.CanvasLeft));
            rectModel.Rectangle.SetBinding(Canvas.TopProperty, nameof(RectModel.CanvasTop));
            rectModel.Rectangle.SetBinding(Shape.StrokeProperty, $"{nameof(TagModel)}.{nameof(TagModel.TagColor)}");
        }

        /// <summary>
        /// 添加标注矩形框
        /// </summary>
        /// <param name="rectModel">新的标注矩形实体</param>
        private void AddRectModel(RectModel rectModel)
        {
            //判断这个矩形实体是否创建过真实绘制
            if (rectModel.Rectangle == null)
            {
                rectModel.Rectangle = new Rectangle()
                {
                    Fill = GetFillColor(this.ViewModel.TagModelSelect.TagColor),
                    StrokeThickness = 1 / this.ViewModel.Scale
                };
                this.SetRectModelBinding(rectModel);
            }

            if (!this.AnnotationCanvas.Children.Contains(rectModel.Rectangle))
            {
                this.AnnotationCanvas.Children.Add(rectModel.Rectangle);
            }
            if (!this.PicturesView.ViewModel.ImgModelSelect.RectModelList.Contains(rectModel))
            {
                this.PicturesView.ViewModel.ImgModelSelect.RectModelList.Add(rectModel);
            }

            PicturesView.UpdateTagSumModelList(this.PicturesView.ViewModel.ImgModelSelect);
        }

        /// <summary>
        /// 获取当前项目所有标注类别
        /// </summary>
        /// <returns></returns>
        public List<TagModel> GetTagModelList()
        {
            return this.ProjectsView.ViewModel.ProjectModelSelect.TagModelList.ToList();
        }

        /// <summary>
        /// 刷新鼠标样式
        /// </summary>
        private void RefreshCursorStyle()
        {
            //拖拽光标
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                this.Cursor = this.DragCursor;
            }
            //拖拽光标
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = this.DragCursor;
            }
            //十字光标
            else if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                this.Cursor = Cursors.Cross;
            }
            //小手光标
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && !(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                this.Cursor = this.HandCursor;
            }
            //设置画笔
            else if ((Keyboard.IsKeyDown(Key.LeftAlt)
                || Keyboard.IsKeyDown(Key.RightAlt))
                || Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = this.DrawCursor;
            }
            else
            {
                this.RefreshRectSizeCursorStyle();
            }

        }

        /// <summary>
        /// 这是刷新矩形调整时鼠标的样式
        /// </summary>
        private void RefreshRectSizeCursorStyle()
        {
            //获取鼠标在标注Canvas上移动的坐标点
            var point = this.ViewModel.AnnotationCanvasMouseMovePosition;
            this.Cursor = this.DrawCursor;
            this.RectResizeEvnets = RectResizeEvnets.None;
            //这里获取矩形选择框
            var rectModelSelectList = this.GetRectModelSelectList();
            if (rectModelSelectList.Count == 0)
            {
                return;
            }

            //获取矩形框实际的宽度 需要考虑这个缩放
            var borderWidth = 2D / this.ViewModel.Scale;

            //相当去在四个角落设置一个长宽4pixel的小矩形块，要考虑这个缩放
            var cornerWidth = 4D / this.ViewModel.Scale;

            //优先判断4个角
            foreach (var item in rectModelSelectList)
            {
                var width = item.Width;
                var height = item.Height;
                var left = item.CanvasLeft;
                var top = item.CanvasTop;
                //判断是不是在左上角
                if (point.X >= left - cornerWidth
                    && point.X <= left + cornerWidth
                    && point.Y >= top - cornerWidth
                    && point.Y <= top + cornerWidth)
                {
                    this.Cursor = Cursors.SizeNWSE;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeLeftTopCorner;
                    return;
                }
                //判断是不是右上角
                else if (point.X >= left + width - cornerWidth
                    && point.X <= left + width + cornerWidth
                    && point.Y >= top - cornerWidth
                    && point.Y <= top + cornerWidth)
                {
                    this.Cursor = Cursors.SizeNESW;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeRightTopCorner;
                    return;
                }
                //判断是不是右下角
                else if (point.X >= left + width - cornerWidth
                    && point.X <= left + width + cornerWidth
                    && point.Y >= top + height - cornerWidth
                    && point.Y <= top + height + cornerWidth)
                {
                    this.Cursor = Cursors.SizeNWSE;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeRightBottomCorner;
                    return;
                }
                //判断是不是左下角
                else if (point.X >= left - cornerWidth
                    && point.X <= left + cornerWidth
                    && point.Y >= top + height - cornerWidth
                    && point.Y <= top + height + cornerWidth)
                {
                    this.Cursor = Cursors.SizeNESW;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeLeftBottomCorner;
                    return;
                }
            }

            //再来判断4个边框
            foreach (var item in rectModelSelectList)
            {
                var width = item.Width;
                var height = item.Height;
                var left = item.CanvasLeft;
                var top = item.CanvasTop;
                //判断是不是在左边框
                if (point.X >= left - borderWidth
                    && point.X <= left + borderWidth
                    && point.Y >= top + cornerWidth
                    && point.Y <= top + height - cornerWidth)
                {
                    this.Cursor = Cursors.SizeWE;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeLeftBorder;
                    return;
                }
                //判断是不是上边框
                else if (point.X > left + cornerWidth
                    && point.X < left + width - cornerWidth
                    && point.Y >= top - borderWidth
                    && point.Y <= top + borderWidth)
                {
                    this.Cursor = Cursors.SizeNS;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeTopBorder;
                    return;
                }
                //判断是不是右边框
                else if (point.X >= left + width - borderWidth
                    && point.X <= left + width + borderWidth
                    && point.Y > top + cornerWidth
                    && point.Y < top + height - cornerWidth)
                {
                    this.Cursor = Cursors.SizeWE;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeRightBorder;
                    return;
                }
                //判断是不是下边框
                else if (point.X > left + cornerWidth
                    && point.X < left + width - cornerWidth
                    && point.Y >= top + height - borderWidth
                    && point.Y <= top + height + borderWidth)
                {
                    this.Cursor = Cursors.SizeNS;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeBottomBorder;
                    return;
                }
            }

            //最后判断是不是内部 中字型矩形 切割成5个矩形分别判断
            foreach (var item in rectModelSelectList)
            {
                var width = item.Width;
                var height = item.Height;
                var left = item.CanvasLeft;
                var top = item.CanvasTop;
                if ((point.X > left + cornerWidth
                    && point.X < left + width - cornerWidth
                    && point.Y > top + cornerWidth
                    && point.Y < top + height - cornerWidth)
                    || (point.X > left + borderWidth
                    && point.X <= left + cornerWidth
                    && point.Y > top + cornerWidth
                    && point.Y < top + height - cornerWidth)
                    || (point.X >= left + width - cornerWidth
                    && point.X < left + width - borderWidth
                    && point.Y > top + cornerWidth
                    && point.Y < top + height - cornerWidth)
                    || (point.X > left + cornerWidth
                    && point.X < left + width - cornerWidth
                    && point.Y > top + borderWidth
                    && point.Y <= top + cornerWidth)
                    || (point.X > left + cornerWidth
                    && point.X < left + width - cornerWidth
                    && point.Y >= top + height - cornerWidth
                    && point.Y < top + height - borderWidth))
                {
                    this.Cursor = Cursors.SizeAll;
                    this.RectResizeEvnets = RectResizeEvnets.ResizeAll;
                    return;
                }
            }
        }

        /// <summary>
        /// 图像拖拽平移变换事件
        /// </summary>
        private void ViewDragEvent()
        {
            this.ViewModel.TransformX = this.ViewModel.CanvasMouseDownTranslateTransformPosition.X + (this.ViewModel.AnnotationCanvasHostMouseMovePosition.X - this.AnnotationCanvasHostMouseDownPosition.X);
            this.ViewModel.TransformY = this.ViewModel.CanvasMouseDownTranslateTransformPosition.Y + (this.ViewModel.AnnotationCanvasHostMouseMovePosition.Y - this.AnnotationCanvasHostMouseDownPosition.Y);
        }


        /// <summary>
        /// 标注Canvas容器鼠标弹起事件
        /// </summary>
        private void AnnotationCanvasHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsAnnotationCanvasHost_MouseDown)
            {
                return;
            }
            this.IsAnnotationCanvasHost_MouseDown = false;
            this.AnnotationCanvasHostMouseDownPosition = default;
            this.AnnotationCanvasMouseDownPosition = default;

            //获取鼠标弹起来时的位置
            var annotationCanvasHostMouseUpPosition = e.GetPosition(this.AnnotationCanvasHost);
            //获取标注Canvas鼠标弹起时的位置
            var annotationCanvasMouseUpPosition = e.GetPosition(this.AnnotationCanvas);

            //这里是用户进行鼠标移动事件
            if (this.IsAnnotationCanvasHostMouseMoveEvent)
            {
                if (this.RectModelDraw != null)
                {
                    this.EndDrawRectModelEvent();
                }
                if (this.MultiSelectRectModelDraw != null)
                {
                    this.EndMultiSelectRectModelDrawEvent();
                }
            }
            else
            {
                //如果没有触发鼠标移动事件
                if (e.ChangedButton == MouseButton.Left && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    this.RetangleSelectEvent(annotationCanvasMouseUpPosition, true);
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    this.RetangleSelectEvent(annotationCanvasMouseUpPosition, false);
                }

                //待处理
                if (this.Cursor == Cursors.Cross)
                {

                }
            }

            if (this.AnnotationCanvasHost_MouseDownCursorHistory != null)
            {
                this.Cursor = this.AnnotationCanvasHost_MouseDownCursorHistory;
                this.AnnotationCanvasHost_MouseDownCursorHistory = null;
            }

            this.IsAnnotationCanvasHostMouseLeftPressDown = false;

            //结束我们的鼠标平移事件
            this.IsAnnotationCanvasHostMouseMoveEvent = false;

            //清空这字典
            this.ResizeRectDataDic.Clear();

            this.UpdateNavRectModelStyle();
            //重新刷新我们的鼠标样式
            this.RefreshCursorStyle();
        }

        /// <summary>
        /// 结束标注矩形多选框绘制事件
        /// </summary>
        private void EndMultiSelectRectModelDrawEvent()
        {
            if (this.MultiSelectRectModelDraw != null)
            {
                this.AnnotationCanvas.Children.Remove(this.MultiSelectRectModelDraw.Rectangle);
            }

            this.RefreshRectModelStyle();

            this.MultiSelectRectModelDraw = null;
        }

        /// <summary>
        /// 处理矩形选择事件
        /// </summary>
        /// <param name="annotationCanvasMouseUpPosition">用户鼠标点击的坐标点</param>
        /// <param name="isMultiSelect">用户是否多选</param>
        private void RetangleSelectEvent(Point point, bool isMultiSelect)
        {
            //用户鼠标按下去弹起来的时候就获取到了该坐标点下的所有矩形
            var selectRectList = this.GetRectModelWithPoint(point);

            if (selectRectList.Count == 0)
            {
                //清空选择历史
                this.SelectRectModelHistoryList = new List<RectModel>();
            }

            //求2个集合是否包含相同的，
            if (selectRectList.Except(this.SelectRectModelHistoryList).Any() || this.SelectRectModelHistoryList.Except(selectRectList).Any())
            {
                this.SelectRectModelHistoryList = selectRectList;
            }

            //这就就获取到我们当前的矩形选择
            var selectRetangleCurrent = this.SelectRectModelHistoryList.FirstOrDefault();
            if (selectRetangleCurrent != null)
            {
                //将选择移动到集合的末尾 做循环选择
                this.SelectRectModelHistoryList.Remove(selectRetangleCurrent);
                this.SelectRectModelHistoryList.Add(selectRetangleCurrent);
            }

            var rectModelSelectList = this.GetRectModelSelectList();

            //如果是多选
            if (isMultiSelect)
            {

                //如果全部都存在已选择里面 则删除
                if (!this.SelectRectModelHistoryList.Except(rectModelSelectList).Any())
                {
                    foreach (var rectModel in this.SelectRectModelHistoryList)
                    {
                        rectModel.IsSelect = false;
                    }
                }
                else
                {
                    if (selectRetangleCurrent != null)
                    {
                        //设置选中 单选保证只有一个
                        if (!rectModelSelectList.Contains(selectRetangleCurrent))
                        {
                            selectRetangleCurrent.IsSelect = true;
                        }
                        else
                        {
                            selectRetangleCurrent.IsSelect = false;
                        }
                    }
                }
            }
            else
            {
                RectModel rectModelSelected = null;
                if (rectModelSelectList.Count == 1)
                {
                    rectModelSelected = rectModelSelectList.FirstOrDefault();
                }

                if (rectModelSelected != null && selectRetangleCurrent == rectModelSelected)
                {
                    selectRetangleCurrent = this.SelectRectModelHistoryList.FirstOrDefault();
                    if (selectRetangleCurrent != null)
                    {
                        //将选择移动到集合的末尾 做循环选择
                        this.SelectRectModelHistoryList.Remove(selectRetangleCurrent);
                        this.SelectRectModelHistoryList.Add(selectRetangleCurrent);
                    }
                }

                this.ClearAllRectModelSelect();

                if (selectRetangleCurrent != null)
                {
                    selectRetangleCurrent.IsSelect = true;
                    this.Cursor = Cursors.SizeAll;
                }
            }

            this.SetRectModelSelect();

            this.RefreshRectModelStyle();
        }

        /// <summary>
        /// 结束鼠标绘画事件
        /// </summary>
        private void EndDrawRectModelEvent()
        {
            this.RectModelDraw.IsSelect = true;
            this.RefreshRectModelStyle();
            this.RectModelDraw = null;
            this.SetRectModelSelect();
        }

        /// <summary>
        /// 获取当前选中的矩形
        /// </summary>
        private RectModel SetRectModelSelect()
        {
            var rectModel = this.GetRectModelSelectList().FirstOrDefault();
            this.ViewModel.RectModelSelect = rectModel;
            return rectModel;
        }

        /// <summary>
        /// 获取当前所有选择的标注矩形框
        /// </summary>
        /// <returns></returns>
        private List<RectModel> GetRectModelSelectList()
        {
            if (this.PicturesView.ViewModel.ImgModelSelect == null)
            {
                return new List<RectModel>();
            }
            return PicturesView.ViewModel.ImgModelSelect.RectModelList.Where(t => t.IsSelect).ToList(); ;
        }

        /// <summary>
        /// 获取当前鼠标点击位置下的所有矩形框
        /// </summary>
        /// <param name="point">当前标注Canvas鼠标点击位置坐标</param>
        /// <returns></returns>
        private List<RectModel> GetRectModelWithPoint(Point point)
        {
            var rectModelList = this.PicturesView.ViewModel.ImgModelSelect.RectModelList;
            return rectModelList
                .Where(t => (point.X >= t.CanvasLeft && point.X <= t.CanvasLeft + t.Width)
            && (point.Y >= t.CanvasTop && point.Y <= t.CanvasTop + t.Height)).ToList();
        }

        /// <summary>
        /// 刷新矩形框样式
        /// </summary>
        private void RefreshRectModelStyle()
        {
            foreach (var rectModel in this.PicturesView.ViewModel.ImgModelSelect.RectModelList)
            {
                if (rectModel.TagModel == null)
                {
                    continue;
                }

                rectModel.Rectangle.Fill = this.GetFillColor(rectModel.TagModel.TagColor);
                rectModel.Rectangle.StrokeThickness = 1 / this.ViewModel.Scale;
                if (rectModel.IsSelect)
                {
                    rectModel.Rectangle.StrokeThickness = 2 / this.ViewModel.Scale;
                }
            }
        }

        /// <summary>
        /// 将字符串颜色转为Brush
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        private Brush GetFillColor(string hexColor)
        {
            var tagColor = (Color)ColorConverter.ConvertFromString(hexColor);
            var fillColor = new SolidColorBrush(Color.FromArgb(this.ViewModel.Opacity, tagColor.R, tagColor.G, tagColor.B));
            return fillColor;
        }

        /// <summary>
        /// 标注Canvas容器鼠标进入事件
        /// </summary>
        private void AnnotationCanvasHost_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PicturesView.SetElementFocus(this.AnnotationCanvasHost);
        }

        /// <summary>
        /// 标注Canvas容器鼠标离开事件
        /// </summary>
        private void AnnotationCanvasHost_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 标注Canvas容器键盘按下事件
        /// </summary>
        private void AnnotationCanvasHost_KeyDown(object sender, KeyEventArgs e)
        {
            //获取用户按键类型
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var rectModelSelectList = this.GetRectModelSelectList();
            switch (key)
            {
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.Tab:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Enter:
                    break;
                //case Key.Return:
                //    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                //case Key.CapsLock:
                //    break;
                case Key.HangulMode:
                    break;
                //case Key.KanaMode:
                //    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                //case Key.KanjiMode:
                //    break;
                case Key.Escape:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.PageUp:
                    break;
                //case Key.Prior:
                //    break;
                //case Key.Next:
                //    break;
                case Key.PageDown:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                #region 键盘方向键
                case Key.Left:
                case Key.Up:
                case Key.Right:
                case Key.Down:
                    if (rectModelSelectList.Count != 0)
                    {
                        this.ResizeRectWithKeyBoardArrowEvent();
                    }
                    else
                    {
                        this.PicturesView.SetImgModelSelectWithArrowKey(key);
                    }
                    break;
                #endregion
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.PrintScreen:
                    break;
                //case Key.Snapshot:
                //    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    this.DeleteRectModelEvent();
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    this.SetTagModelSelectWithShortCut($"{(int)key - (int)Key.D0}");
                    break;
                case Key.A:
                    if (this.Cursor == this.HandCursor)
                    {
                        this.RectModelSelectAllEvent();
                    }
                    else if (this.Cursor == this.DragCursor && rectModelSelectList.Count == 0)
                    {
                        this.PicturesView.SetImgModelSelectWithArrowKey(Key.Left);
                    }
                    if (rectModelSelectList.Count != 0 && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        this.ResizeRectWithKeyBoardArrowEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.B:
                    SetTagModelSelectWithShortCut(GetLetter((int)key));
                    break;
                case Key.C:
                    //复制矩形
                    if (this.Cursor == this.HandCursor)
                    {
                        this.CopyRectModelEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.D:
                    if (this.Cursor == this.HandCursor)
                    {
                        this.DeleteRectModelEvent();
                    }
                    else if (this.Cursor == this.DragCursor && rectModelSelectList.Count == 0)
                    {
                        this.PicturesView.SetImgModelSelectWithArrowKey(Key.Right);
                    }
                    else if (rectModelSelectList.Count != 0)
                    {
                        this.ResizeRectWithKeyBoardArrowEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.P:
                case Key.Q:
                case Key.R:
                    SetTagModelSelectWithShortCut(GetLetter((int)key));
                    break;
                case Key.S:
                    if (this.Cursor == this.DragCursor && rectModelSelectList.Count == 0)
                    {
                        this.PicturesView.SetImgModelSelectWithArrowKey(Key.Down);
                    }
                    else if (rectModelSelectList.Count != 0)
                    {
                        this.ResizeRectWithKeyBoardArrowEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.T:
                case Key.U:
                    SetTagModelSelectWithShortCut(GetLetter((int)key));
                    break;
                case Key.V:
                    //粘贴
                    if (this.Cursor == this.HandCursor)
                    {
                        this.PasteRectModelEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.W:
                    if (this.Cursor == this.DragCursor && rectModelSelectList.Count == 0)
                    {
                        this.PicturesView.SetImgModelSelectWithArrowKey(Key.Up);
                    }
                    else if (rectModelSelectList.Count != 0)
                    {
                        this.ResizeRectWithKeyBoardArrowEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.X:
                    //剪切
                    if (this.Cursor == this.HandCursor)
                    {
                        this.CutRectModelEvent();
                    }
                    else
                    {
                        SetTagModelSelectWithShortCut(GetLetter((int)key));
                    }
                    break;
                case Key.Y:
                case Key.Z:
                    SetTagModelSelectWithShortCut(GetLetter((int)key));
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    this.SetTagModelSelectWithShortCut($"{key - (int)Key.NumPad0}");
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    if (!(Mouse.MiddleButton == MouseButtonState.Released))
                    {
                        if (this.ShiftDownCursorHistory == null)
                        {
                            this.ShiftDownCursorHistory = this.Cursor;
                            this.Cursor = Cursors.Cross;
                            if (Mouse.LeftButton == MouseButtonState.Released)
                            {

                            }
                        }
                    }
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    if (Keyboard.IsKeyDown(Key.A))
                    {

                    }
                    else if (Keyboard.IsKeyDown(Key.D))
                    {
                        this.PicturesView.DeleteImgSelect();
                    }
                    else
                    {
                        if (!(Mouse.MiddleButton == MouseButtonState.Released
                            || Keyboard.IsKeyDown(Key.LeftAlt)
                            || Keyboard.IsKeyDown(Key.RightAlt)
                            || Keyboard.IsKeyDown(Key.LeftShift)
                            || Keyboard.IsKeyDown(Key.RightShift)))
                        {
                            if (this.CtrlKeyDownCursorHistory == null)
                            {
                                this.CtrlKeyDownCursorHistory = this.Cursor;
                                this.Cursor = this.HandCursor;
                            }
                        }
                    }
                    break;
                case Key.LeftAlt:
                case Key.RightAlt:
                    if (!(Mouse.MiddleButton == MouseButtonState.Released))
                    {
                        if (this.AltKeyDownCursorHistory == null)
                        {
                            this.AltKeyDownCursorHistory = this.Cursor;
                            this.Cursor = this.DrawCursor;
                        }
                    }
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                //case Key.OemSemicolon:
                //    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                //case Key.OemQuestion:
                //    break;
                case Key.Oem3:
                    break;
                //case Key.OemTilde:
                //    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                //case Key.OemOpenBrackets:
                //    break;
                case Key.Oem5:
                    break;
                //case Key.OemPipe:
                //    break;
                case Key.Oem6:
                    break;
                //case Key.OemCloseBrackets:
                //    break;
                case Key.Oem7:
                    break;
                //case Key.OemQuotes:
                //    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                //case Key.OemBackslash:
                //    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.DbeAlphanumeric:
                    break;
                //case Key.OemAttn:
                //    break;
                case Key.DbeKatakana:
                    break;
                //case Key.OemFinish:
                //    break;
                case Key.DbeHiragana:
                    break;
                //case Key.OemCopy:
                //    break;
                case Key.DbeSbcsChar:
                    break;
                //case Key.OemAuto:
                //    break;
                case Key.DbeDbcsChar:
                    break;
                //case Key.OemEnlw:
                //    break;
                case Key.DbeRoman:
                    break;
                //case Key.OemBackTab:
                //    break;
                case Key.Attn:
                    break;
                //case Key.DbeNoRoman:
                //    break;
                case Key.CrSel:
                    break;
                //case Key.DbeEnterWordRegisterMode:
                //    break;
                case Key.DbeEnterImeConfigureMode:
                    break;
                //case Key.ExSel:
                //    break;
                case Key.DbeFlushString:
                    break;
                //case Key.EraseEof:
                //    break;
                case Key.DbeCodeInput:
                    break;
                //case Key.Play:
                //    break;
                case Key.DbeNoCodeInput:
                    break;
                //case Key.Zoom:
                //    break;
                case Key.DbeDetermineString:
                    break;
                //case Key.NoName:
                //    break;
                case Key.DbeEnterDialogConversionMode:
                    break;
                //case Key.Pa1:
                //    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
                default:
                    break;
            }
            this.RefreshCursorStyle();
            e.Handled = true;
        }

        /// <summary>
        /// 根据键盘按下的枚举值计算出当前枚举值对应的ASCII编码
        /// </summary>

        private string GetLetter(int num)
        {
            // 65 是 'A' 的 ASCII 码
            // 我们需要将数字范围 44-69 映射到 'A'-'Z' 范围内
            // 因此，我们需要减去 44-65 的差值（即减去 21）
            return ((char)(num - (44 - 65))).ToString();
        }

        /// <summary>
        /// 根据快捷键选择分类标签
        /// </summary>
        /// <param name="shortCut">快捷键</param>
        private void SetTagModelSelectWithShortCut(string shortCut)
        {
            if (Console.CapsLock)
            {
                shortCut = shortCut.ToUpper();
            }
            else
            {
                shortCut = shortCut.ToLower();
            }

            var tagModelList = this.GetTagModelList();
            var firstOrDefault = tagModelList.FirstOrDefault(t => t.ShortCut == shortCut);
            if (firstOrDefault != null)
            {
                firstOrDefault.IsSelect = true;
                this.ViewModel.TagModelSelect = firstOrDefault;
            }
        }

        /// <summary>
        /// 标注矩形框剪切事件
        /// </summary>
        private void CutRectModelEvent()
        {
            var rectModelSelectList = this.GetRectModelSelectList();
            if (rectModelSelectList.Count == 0)
            {
                return;
            }
            this.CopyRectModel(true);
        }

        /// <summary>
        /// 标注矩形框粘贴事件
        /// </summary>
        private void PasteRectModelEvent()
        {
            this.ClearAllRectModelSelect();
            var rectModelList = this.GetRectModelList();
            foreach (var item in this.RectModelCopyList)
            {
                var itemClone = this.RectModelClone(item);
                itemClone = this.RefreshRectModelCloneOffset(rectModelList, itemClone);
                this.AddRectModel(itemClone);
            }
            this.RefreshRectModelStyle();
        }

        /// <summary>
        /// 刷新标复制后的标注矩形框偏移位置
        /// </summary>
        /// <param name="rectModelList">当前标注图像拥有的所有标注矩形框</param>
        /// <param name="rectModelClone">复制后的矩形框</param>
        /// <returns></returns>
        private RectModel RefreshRectModelCloneOffset(List<RectModel> rectModelList, RectModel rectModelClone)
        {
            var width = rectModelClone.Width;
            var height = rectModelClone.Height;
            var canvasLeft = rectModelClone.CanvasLeft;
            var canvasTop = rectModelClone.CanvasTop;
            if (rectModelList.Any(t => t.CanvasTop == canvasTop
            && t.CanvasLeft == canvasLeft
            && t.Width == width
            && t.Height == height))
            {
                //如果存在我们设置一个偏移位置
                rectModelClone.CanvasLeft = canvasLeft + 20 / this.ViewModel.Scale;
                rectModelClone.CanvasTop = canvasTop + 20 / this.ViewModel.Scale;
                this.RefreshRectModelCloneOffset(rectModelList, rectModelClone);
            }
            return rectModelClone;
        }

        /// <summary>
        /// 复制当前选择的矩形框
        /// </summary>
        /// <param name="rectModel">被复制的矩形框</param>
        /// <returns></returns>
        private RectModel RectModelClone(RectModel rectModel)
        {
            RectModel rectModelClone = new RectModel(Guid.NewGuid().ToString(), rectModel.PictureId, this.ProjectsView.ViewModel.ProjectModelSelect.Id)
            {
                Width = rectModel.Width,
                Height = rectModel.Height,
                CanvasLeft = rectModel.CanvasLeft,
                CanvasTop = rectModel.CanvasTop,
                Angle = rectModel.Angle,
                Rectangle = new Rectangle()
                {
                    StrokeThickness = rectModel.Rectangle.StrokeThickness,
                },
                TagModel = rectModel.TagModel,
                ProjectId = rectModel.ProjectId,
                IsSelect = true,
                IsSaved = false,
            };

            this.SetRectModelBinding(rectModelClone);
            return rectModelClone;
        }

        /// <summary>
        /// 复制标注矩形框事件
        /// </summary>
        private void CopyRectModelEvent()
        {
            if (this.ViewModel.RectModelSelect == null)
            {
                return;
            }
            this.CopyRectModel(false);
        }


        /// <summary>
        /// 复制标注矩形框方法
        /// </summary>
        /// <param name="isRemove">复制后是否删除，比如剪切时需要移除</param>
        private void CopyRectModel(bool isRemove)
        {
            this.RectModelCopyList.Clear();
            var rectModelSelectList = this.GetRectModelSelectList();

            foreach (var item in rectModelSelectList)
            {
                if (!this.RectModelCopyList.Contains(item))
                {
                    this.RectModelCopyList.Add(item);
                }
                if (isRemove)
                {
                    this.DeleteRectModel(item);
                }
            }

        }

        /// <summary>
        /// 使用键盘事件更改标注矩形框尺寸事件
        /// </summary>
        private void ResizeRectWithKeyBoardArrowEvent()
        {
            if (this.Cursor == null)
            {
                return;
            }
            var rectModelSelectList = this.GetRectModelSelectList();

            if (Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.W))
            {
                foreach (var item in rectModelSelectList)
                {
                    item.CanvasTop = item.CanvasTop - 1.42;
                }
            }
            else if (Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.S))
            {
                foreach (var item in rectModelSelectList)
                {
                    item.CanvasTop = item.CanvasTop + 1.42;
                }
            }
            else if (Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.A))
            {
                foreach (var item in rectModelSelectList)
                {
                    item.CanvasLeft = item.CanvasLeft - 1.42;
                }
            }
            else if (Keyboard.IsKeyDown(Key.Right) || Keyboard.IsKeyDown(Key.D))
            {
                foreach (var item in rectModelSelectList)
                {
                    item.CanvasLeft = item.CanvasLeft + 1.42;
                }
            }


        }

        /// <summary>
        /// 标注矩形框全选事件 用户按下Ctrl+A进行全选 再次按下进行反选
        /// </summary>
        private void RectModelSelectAllEvent()
        {
            var rectModelList = this.GetRectModelList();
            var rectModelSelectList = this.GetRectModelSelectList();

            if (rectModelSelectList.Count == rectModelList.Count)
            {
                this.ClearAllRectModelSelect();
            }
            else
            {
                foreach (var rectModel in rectModelList)
                {
                    rectModel.IsSelect = true;
                }
            }

            this.SetRectModelSelect();
            this.RefreshRectModelStyle();
        }

        /// <summary>
        /// 获取当前标注图像拥有的所有矩形框
        /// </summary>
        /// <returns></returns>
        private List<RectModel> GetRectModelList()
        {
            return this.PicturesView.ViewModel.ImgModelSelect.RectModelList.ToList();
        }

        /// <summary>
        /// 删除标注矩形框事件
        /// </summary>
        private void DeleteRectModelEvent()
        {
            var rectModelSelectList = this.GetRectModelSelectList();
            if (rectModelSelectList.Count == 0)
            {
                return;
            }
            foreach (var rectModel in rectModelSelectList)
            {
                this.DeleteRectModel(rectModel);
            }
        }

        /// <summary>
        /// 将矩形框从图像矩形列表移除，同时从标注Canvas上移除
        /// </summary>
        /// <param name="rectModel"></param>
        private void DeleteRectModel(RectModel rectModel)
        {
            if (this.AnnotationCanvas.Children.Contains(rectModel.Rectangle))
            {
                this.AnnotationCanvas.Children.Remove(rectModel.Rectangle);
            }
            if (this.PicturesView.ViewModel.ImgModelSelect.RectModelList.Contains(rectModel))
            {
                this.PicturesView.ViewModel.ImgModelSelect.RectModelList.Remove(rectModel);
            }

            this.ViewModel.RectModelSelect = null;

            this.PicturesView.UpdateTagSumModelList(this.PicturesView.ViewModel.ImgModelSelect);
        }

        /// <summary>
        /// 标注Canvas容器键盘弹起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnnotationCanvasHost_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.AnnotationCanvasHost.IsMouseOver)
            {
                return;
            }

            //获取用户按键类型
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            switch (key)
            {
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.Tab:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Enter:
                    break;
                //case Key.Return:
                //    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                //case Key.CapsLock:
                //    break;
                case Key.HangulMode:
                    break;
                //case Key.KanaMode:
                //    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                //case Key.KanjiMode:
                //    break;
                case Key.Escape:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.PageUp:
                    break;
                //case Key.Prior:
                //    break;
                //case Key.Next:
                //    break;
                case Key.PageDown:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                #region 键盘方向键
                case Key.Left:
                    break;
                case Key.Up:
                    break;
                case Key.Right:
                    break;
                case Key.Down:
                    break;
                #endregion
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.PrintScreen:
                    break;
                //case Key.Snapshot:
                //    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                    break;
                case Key.D1:
                    break;
                case Key.D2:
                    break;
                case Key.D3:
                    break;
                case Key.D4:
                    break;
                case Key.D5:
                    break;
                case Key.D6:
                    break;
                case Key.D7:
                    break;
                case Key.D8:
                    break;
                case Key.D9:
                    break;
                case Key.A:
                    break;
                case Key.B:
                    break;
                case Key.C:
                    break;
                case Key.D:
                    break;
                case Key.E:
                    break;
                case Key.F:
                    break;
                case Key.G:
                    break;
                case Key.H:
                    break;
                case Key.I:
                    break;
                case Key.J:
                    break;
                case Key.K:
                    break;
                case Key.L:
                    break;
                case Key.M:
                    break;
                case Key.N:
                    break;
                case Key.O:
                    break;
                case Key.P:
                    break;
                case Key.Q:
                    break;
                case Key.R:
                    break;
                case Key.S:
                    break;
                case Key.T:
                    break;
                case Key.U:
                    break;
                case Key.V:
                    break;
                case Key.W:
                    break;
                case Key.X:
                    break;
                case Key.Y:
                    break;
                case Key.Z:
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                    break;
                case Key.NumPad1:
                    break;
                case Key.NumPad2:
                    break;
                case Key.NumPad3:
                    break;
                case Key.NumPad4:
                    break;
                case Key.NumPad5:
                    break;
                case Key.NumPad6:
                    break;
                case Key.NumPad7:
                    break;
                case Key.NumPad8:
                    break;
                case Key.NumPad9:
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                    break;
                case Key.RightShift:
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    if (this.CtrlKeyDownCursorHistory != null)
                    {
                        if (Mouse.MiddleButton == MouseButtonState.Pressed)
                        {
                            this.AnnotationCanvasHost_MouseDownCursorHistory = this.CtrlKeyDownCursorHistory;
                        }
                        else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                        {
                            this.AltKeyDownCursorHistory = this.CtrlKeyDownCursorHistory;
                        }
                        else
                        {
                            this.Cursor = this.CtrlKeyDownCursorHistory;
                        }
                    }
                    break;
                case Key.LeftAlt:
                    break;
                case Key.RightAlt:
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                //case Key.OemSemicolon:
                //    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                //case Key.OemQuestion:
                //    break;
                case Key.Oem3:
                    break;
                //case Key.OemTilde:
                //    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                //case Key.OemOpenBrackets:
                //    break;
                case Key.Oem5:
                    break;
                //case Key.OemPipe:
                //    break;
                case Key.Oem6:
                    break;
                //case Key.OemCloseBrackets:
                //    break;
                case Key.Oem7:
                    break;
                //case Key.OemQuotes:
                //    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                //case Key.OemBackslash:
                //    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.DbeAlphanumeric:
                    break;
                //case Key.OemAttn:
                //    break;
                case Key.DbeKatakana:
                    break;
                //case Key.OemFinish:
                //    break;
                case Key.DbeHiragana:
                    break;
                //case Key.OemCopy:
                //    break;
                case Key.DbeSbcsChar:
                    break;
                //case Key.OemAuto:
                //    break;
                case Key.DbeDbcsChar:
                    break;
                //case Key.OemEnlw:
                //    break;
                case Key.DbeRoman:
                    break;
                //case Key.OemBackTab:
                //    break;
                case Key.Attn:
                    break;
                //case Key.DbeNoRoman:
                //    break;
                case Key.CrSel:
                    break;
                //case Key.DbeEnterWordRegisterMode:
                //    break;
                case Key.DbeEnterImeConfigureMode:
                    break;
                //case Key.ExSel:
                //    break;
                case Key.DbeFlushString:
                    break;
                //case Key.EraseEof:
                //    break;
                case Key.DbeCodeInput:
                    break;
                //case Key.Play:
                //    break;
                case Key.DbeNoCodeInput:
                    break;
                //case Key.Zoom:
                //    break;
                case Key.DbeDetermineString:
                    break;
                //case Key.NoName:
                //    break;
                case Key.DbeEnterDialogConversionMode:
                    break;
                //case Key.Pa1:
                //    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
                default:
                    break;
            }

            this.RefreshCursorStyle();
            e.Handled = true;
        }

        /// <summary>
        /// 标注矩形框透明度值改变事件
        /// </summary>
        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            this.RefreshRectModelStyle();
        }

        /// <summary>
        /// 新增标注类别点击事件
        /// </summary>
        private void BtnAddTagModel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            this.ViewModel.CRUD = CRUD.Add;
            ShowTagEditPopView(this.TagModelListHost, 5);
        }

        /// <summary>
        /// 显示标注类别增改视图
        /// </summary>
        /// <param name="placementTarget">增改视图依附目标</param>
        /// <param name="horizontalOffset">水平偏移位置</param>
        public void ShowTagEditPopView(UIElement placementTarget, int horizontalOffset = 5)
        {
            this.ViewModel.TagModelEdit = new TagModel(Guid.NewGuid().ToString(), this.ProjectsView.ViewModel.ProjectModelSelect.Id);
            switch (this.ViewModel.CRUD)
            {
                case CRUD.Add:
                    this.ViewModel.TagModelEdit = this.InitTagModel();
                    break;
                case CRUD.Update:
                    this.ViewModel.TagModelSelect.CloneTo(this.ViewModel.TagModelEdit);
                    break;
            }
            if (TagEditPopView != null)
            {
                this.RootHost.Children.Remove(TagEditPopView);
            }

            TagEditPopView = new TagEditPopView(this);
            TagEditPopView.PlacementTarget = placementTarget;
            TagEditPopView.Placement = PlacementMode.Right;
            TagEditPopView.StaysOpen = false;
            TagEditPopView.HorizontalOffset = horizontalOffset;
            //TagEditPopView.VerticalOffset = -22;
            TagEditPopView.OnOKCallBack -= TagEditPopView_OnOKCallBack;
            TagEditPopView.OnOKCallBack += TagEditPopView_OnOKCallBack;
            this.RootHost.Children.Add(TagEditPopView);
            TagEditPopView.IsOpen = true;
        }

        /// <summary>
        /// 标注类别增改视图用户点击确认后回调事件
        /// </summary>
        private void TagEditPopView_OnOKCallBack()
        {
            //假如有其他事件可以在这里进行处理

        }

        /// <summary>
        /// 初始化一个新的标注类别
        /// </summary>
        /// <returns></returns>
        public TagModel InitTagModel()
        {
            var tagModel = new TagModel(Guid.NewGuid().ToString(), this.ProjectsView.ViewModel.ProjectModelSelect.Id);
            tagModel.IsShortCutAuto = true;
            //获取所有标注类
            var tagModelList = this.GetTagModelList();
            //获取所有标注快捷键
            var shortCutList = tagModelList.Select(t => t.ShortCut).ToList();
            tagModel.ShortCut = ShortCutHelper.GetShortCutKey(shortCutList);
            tagModel.IsSaved = false;

            string tagColor = null;
            do
            {
                var lastTagModel = tagModelList.LastOrDefault();
                tagColor = GenerateRandomColor();
            } while (tagModelList.Any(t => t.TagColor == tagColor) || tagColor == null);

            tagModel.TagColor = tagColor;
            return tagModel;
        }

        /// <summary>
        /// 生成随机颜色
        /// </summary>
        /// <returns></returns>
        private string GenerateRandomColor()
        {
            Random random = new Random();
            byte r = (byte)random.Next(0, 256);
            byte g = (byte)random.Next(0, 256);
            byte b = (byte)random.Next(0, 256);
            byte a = 255;
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 为了解决用户没有鼠标滚轮的情况下，依然可以选择点击缩小按钮调整标注视窗的缩放
        /// 这里缩小
        /// </summary>
        private void BtnScaleDown_Click(object sender, RoutedEventArgs e)
        {
            //先更新缩放比例
            this.UpdateScale(false);
            this.UpdateAnnotationCanvasScaleWithCanvasHostCenter();
        }

        /// <summary>
        /// 为了解决用户没有鼠标滚轮的情况下，依然可以选择点击缩小按钮调整标注视窗的缩放
        /// 这里是放大
        /// </summary>
        private void BtnScaleUp_Click(object sender, RoutedEventArgs e)
        {
            //先更新缩放比例
            this.UpdateScale(true);
            this.UpdateAnnotationCanvasScaleWithCanvasHostCenter();
        }

        /// <summary>
        /// 视窗缩放Slider发生变化时触发的事件
        /// </summary>
        private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ViewModel == null)
            {
                return;
            }
            if (IsNavScale)
            {
                this.UpdateAnnotationCanvasScaleWithCanvasHostCenter();
            }
            this.UpdateNavRectModelStyle();
        }

        /// <summary>
        /// 更具标注Canvas当前工作中心店改变视窗的缩放
        /// </summary>
        private void UpdateAnnotationCanvasScaleWithCanvasHostCenter()
        {
            if (this.AnnotationCanvas.RenderTransform.Inverse == null)
            {
                return;
            }
            //获取当前视窗的长宽
            var canvasHostActualWidth = this.AnnotationCanvasHost.ActualWidth;
            var canvasHostActualHeight = this.AnnotationCanvasHost.ActualHeight;
            Point zoomCenter = new Point(canvasHostActualWidth / 2, canvasHostActualHeight / 2);
            zoomCenter = this.AnnotationCanvas.RenderTransform.Inverse.Transform(zoomCenter);
            this.UpdateAnnotaionCanvasScale(zoomCenter);
        }


        #region 这是导航器Canvas事件

        /// <summary>
        /// 导航器Canvas容器鼠标移入
        /// </summary>
        private void NavCanvasHost_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        /// <summary>
        /// 导航器Canvas容器鼠标离开事件
        /// </summary>
        private void NavCanvasHost_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private bool IsNavCanvasHost_MouseDown;
        /// <summary>
        /// 导航器Canvas容器鼠标按下事件
        /// </summary>
        private void NavCanvasHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsNavCanvasHost_MouseDown = true;
        }

        /// <summary>
        /// 更新标注Canvas的工作中心
        /// </summary>
        /// <param name="imgViewWorkCenterPoint">要更新的图像工作视窗中心点</param>
        private void UpdateAnnotaionCanvasWorkCenter(Point imgViewWorkCenterPoint)
        {
            //获取标注视窗的宽度和高度
            var annotationCanvasHostActualWidth = this.AnnotationCanvasHost.ActualWidth;
            var annotationCanvasHostActualHeight = this.AnnotationCanvasHost.ActualHeight;

            //获取标注视窗的正中心
            var canvasHostCenterX = annotationCanvasHostActualWidth / 2;
            var canvasHostCenterY = annotationCanvasHostActualHeight / 2;

            this.ViewModel.TransformX = canvasHostCenterX - imgViewWorkCenterPoint.X;
            this.ViewModel.TransformY = canvasHostCenterY - imgViewWorkCenterPoint.Y;

            this.ViewModel.CenterX = imgViewWorkCenterPoint.X;
            this.ViewModel.CenterY = imgViewWorkCenterPoint.Y;
            this.UpdateNavRectModelStyle();
        }

        /// <summary>
        /// 更新导航矩形框的样式
        /// </summary>
        private void UpdateNavRectModelStyle()
        {
            if (this.ViewModel.NavRectModel == null)
            {
                return;
            }
            if (this.AnnotationCanvas.RenderTransform.Inverse == null)
            {
                return;
            }
            var annotationCanvasHostActualWidth = this.AnnotationCanvasHost.ActualWidth;
            var annotationCanvasHostActualHeight = this.AnnotationCanvasHost.ActualHeight;
            var navImgActualWidth = this.NavImg.ActualWidth;
            var navImgActualHeight = this.NavImg.ActualHeight;
            var imgViewActualWidth = this.ImgView.ActualWidth;
            var imgViewActualHeight = this.ImgView.ActualHeight;

            //获取到当前图像的实际显示大小
            var imgViewScaleWidth = imgViewActualWidth * this.ViewModel.Scale;
            var imgViewScaleHeight = imgViewActualHeight * this.ViewModel.Scale;

            //获取我们当前视窗的中心点
            var canvasHostCenterX = annotationCanvasHostActualWidth / 2;
            var canvasHostCenterY = annotationCanvasHostActualHeight / 2;
            var canvasHostCenterPoint = new Point(annotationCanvasHostActualWidth / 2, annotationCanvasHostActualHeight / 2);
            //获取标注Canvas当前标注视窗正中心的实际坐标
            var canvasHostCenterPointInverse = this.AnnotationCanvas.RenderTransform.Inverse.Transform(canvasHostCenterPoint);

            //这不就获取到了导航器上图像的实际工作中心了吗
            var navRectModelCenterX = canvasHostCenterPointInverse.X / imgViewActualWidth * navImgActualWidth;
            var navRectModelCenterY = canvasHostCenterPointInverse.Y / imgViewActualHeight * navImgActualHeight;

            //再计算我们导航器矩形框的实际尺寸
            var navRectWidth = annotationCanvasHostActualWidth / imgViewScaleWidth * navImgActualWidth;
            var navRectHeight = annotationCanvasHostActualHeight / imgViewScaleHeight * navImgActualHeight;

            this.ViewModel.NavRectModel.Width = Math.Max(navRectWidth, 5);
            this.ViewModel.NavRectModel.Height = Math.Max(navRectHeight, 5);
            this.ViewModel.NavRectModel.CanvasLeft = navRectModelCenterX - this.ViewModel.NavRectModel.Width / 2;
            this.ViewModel.NavRectModel.CanvasTop = navRectModelCenterY - this.ViewModel.NavRectModel.Height / 2;
        }

        /// <summary>
        /// 根据导航视窗的坐标点获取标注视窗的工作中心店
        /// </summary>
        /// <param name="navImgMouseDownPosition">导航Canvas上鼠标按下时的坐标点</param>
        /// <returns></returns>
        private Point GetImgViewWorkCenterPointWithNavImgPoint(Point navImgMouseDownPosition)
        {
            var navImgActualWidth = this.NavImg.ActualWidth;
            var navImgActualHeight = this.NavImg.ActualHeight;
            var imgViewActualWidth = this.ImgView.ActualWidth;
            var imgViewActualHeight = this.ImgView.ActualHeight;

            //获取点击坐标点的比例
            var xRatio = navImgMouseDownPosition.X / navImgActualWidth;
            var yRatio = navImgMouseDownPosition.Y / navImgActualHeight;
            return new Point(imgViewActualWidth * xRatio, imgViewActualHeight * yRatio);
        }

        /// <summary>
        /// 导航Canvas容器鼠标弹起事件
        /// </summary>
        private void NavCanvasHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsNavCanvasHost_MouseDown)
            {
                return;
            }
            this.IsNavCanvasHost_MouseDown = false;
            //获取用户点击图片上的位置坐标
            var navImgMouseDownPosition = e.GetPosition(this.NavImg);
            var imgViewWorkCenterPoint = this.GetImgViewWorkCenterPointWithNavImgPoint(navImgMouseDownPosition);
            this.UpdateAnnotaionCanvasWorkCenter(imgViewWorkCenterPoint);
        }

        /// <summary>
        /// 导航Canvas容器鼠标滚轮事件
        /// </summary>
        private void NavCanvasHost_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.UpdateScale(e.Delta > 0);
            this.UpdateAnnotationCanvasScaleWithCanvasHostCenter();
            this.UpdateNavRectModelStyle();
        }

        /// <summary>
        /// 导航Canvas容器键盘按下事件
        /// </summary>
        private void NavCanvasHost_KeyDown(object sender, KeyEventArgs e)
        {

        }

        /// <summary>
        /// 导航Canvas容器键盘弹起事件
        /// </summary>
        private void NavCanvasHost_KeyUp(object sender, KeyEventArgs e)
        {

        }

        /// <summary>
        /// 导航Canvas容器鼠标移动事件
        /// </summary>
        private void NavCanvasHost_MouseMove(object sender, MouseEventArgs e)
        {
            var navImgMouseMovePosition = e.GetPosition(this.NavImg);
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                var imgViewWorkCenterPoint = this.GetImgViewWorkCenterPointWithNavImgPoint(navImgMouseMovePosition);
                this.UpdateAnnotaionCanvasWorkCenter(imgViewWorkCenterPoint);
            }
        }
        #endregion

        /// <summary>
        /// 编辑标注类别点击事件
        /// </summary>
        private void BtnTagModelEidt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var radioButton = button.TryFindParent<RadioButton>();
            this.ViewModel.CRUD = CRUD.Update;
            this.ShowTagEditPopView(radioButton, 8);
        }

        /// <summary>
        /// 删除标注类别点击事件
        /// </summary>
        private async void BtnTagModelDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagModelSelect = this.ViewModel.TagModelSelect;
            var messageBoxResult = await this.MessageBox.ShowMessageAsync("请注意！所有图像实例该标签分类将会全部删除，你确定要这么操作吗?", "系统提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var validLoginResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
                {
                    this.DeleteTagModel(tagModelSelect);
                    return OperateResult.CreateSuccessResult();
                });
            }
        }

        /// <summary>
        /// 删除标注类别方法
        /// </summary>
        /// <param name="tagModel">要删除的类别对象</param>
        public void DeleteTagModel(TagModel tagModel)
        {
            ProjectsView projectsView = null;
            this.Dispatcher.Invoke(() =>
            {
                projectsView = this.ProjectsView;
            });

            foreach (var imgModel in projectsView.ViewModel.ProjectModelSelect.ImgModelList)
            {
                var rectModelList = imgModel.RectModelList.ToList();
                foreach (var rectModel in rectModelList)
                {
                    if (rectModel.TagModel == tagModel)
                    {

                        this.Dispatcher.Invoke(() =>
                        {
                            imgModel.RectModelList.Remove(rectModel);
                            if (this.AnnotationCanvas.Children.Contains(rectModel.Rectangle))
                            {
                                this.AnnotationCanvas.Children.Remove(rectModel.Rectangle);
                            }
                        });
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        projectsView.ViewModel.ProjectModelSelect.TagModelList.Remove(tagModel);
                    });
                    projectsView.ViewModel.ProjectModelSelect.IsSaved = false;
                    this.ViewModel.TagModelSelect = null;
                }
            }
        }

        /// <summary>
        /// 标注类别列表鼠标双击事件
        /// </summary>
        private async void RadioBtnTagModel_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var radioButton = sender as RadioButton;
            this.ViewModel.CRUD = CRUD.Update;
            await Task.Delay(200);
            ShowTagEditPopView(radioButton, 8);
        }


        /// <summary>
        /// 标注类别列表选中时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioBtnTagModel_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            var tagModel = radioButton.Tag as TagModel;
            this.ViewModel.TagModelSelect = tagModel;
        }


        /// <summary>
        /// 用户选择标注矩形框后 通过修改X,Y,Width,Height后触发值变化回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="valueInput"></param>
        private void RectModel_OnValueInputChanged(RSNumberic sender, double valueInput)
        {
            var rectModelSelectList = this.GetRectModelSelectList();
            switch (sender.Name)
            {
                case "TxtCanvasLeft":
                    foreach (var rectModel in rectModelSelectList)
                    {
                        rectModel.CanvasLeft = valueInput;
                    }
                    break;
                case "TxtCanvasTop":
                    foreach (var rectModel in rectModelSelectList)
                    {
                        rectModel.CanvasTop = valueInput;
                    }
                    break;
                case "TxtWidth":
                    foreach (var rectModel in rectModelSelectList)
                    {
                        rectModel.Width = valueInput;
                    }
                    break;
                case "TxtHeight":
                    foreach (var rectModel in rectModelSelectList)
                    {
                        rectModel.Height = valueInput;
                    }
                    break;
            }
        }
    }
}
