using NPOI.SS.Formula.Functions;
using RS.Commons;
using RS.Models;
using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;

namespace RS.HMI.Client.Controls
{
    /// <summary>
    /// RSImgVerify.xaml 的交互逻辑
    /// </summary>
    public partial class RSImgVerify : UserControl
    {
        private Thumb PART_BtnSlider { get; set; }
        private Thumb PART_BtnImgSlider { get; set; }
        private Canvas PART_BtnSliderHost { get; set; }

        private Border PART_BtnImgSliderHost { get; set; }
        public RSImgVerify()
        {
            InitializeComponent();
        }


        [Browsable(false)]
        [Description("滑动按钮滑动时背景宽度")]
        public double SliderMaskWidth
        {
            get { return (double)GetValue(SliderMaskWidthProperty); }
            set { SetValue(SliderMaskWidthProperty, value); }
        }

        public static readonly DependencyProperty SliderMaskWidthProperty =
            DependencyProperty.Register("SliderMaskWidth", typeof(double), typeof(RSImgVerify), new PropertyMetadata(0D));



        [Description("滑动验证码的背景图")]
        public ImageSource ImgVerifyBackground
        {
            get { return (ImageSource)GetValue(ImgVerifyBackgroundProperty); }
            set { SetValue(ImgVerifyBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ImgVerifyBackgroundProperty =
            DependencyProperty.Register("ImgVerifyBackground", typeof(ImageSource), typeof(RSImgVerify), new PropertyMetadata(null));



        [Description("图像按钮背景")]
        public ImageSource BtnImgSource
        {
            get { return (ImageSource)GetValue(BtnImgSourceProperty); }
            set { SetValue(BtnImgSourceProperty, value); }
        }

        public static readonly DependencyProperty BtnImgSourceProperty =
            DependencyProperty.Register("BtnImgSource", typeof(ImageSource), typeof(RSImgVerify), new PropertyMetadata(null));




        [Description("图像按钮宽度")]
        public double BtnImgWidth
        {
            get { return (double)GetValue(BtnImgWidthProperty); }
            set { SetValue(BtnImgWidthProperty, value); }
        }

        public static readonly DependencyProperty BtnImgWidthProperty =
            DependencyProperty.Register("BtnImgWidth", typeof(double), typeof(RSImgVerify), new PropertyMetadata(20D));



        [Description("图像按钮高度")]
        public double BtnImgHeight
        {
            get { return (double)GetValue(BtnImgHeightProperty); }
            set { SetValue(BtnImgHeightProperty, value); }
        }

        public static readonly DependencyProperty BtnImgHeightProperty =
            DependencyProperty.Register("BtnImgHeight", typeof(double), typeof(RSImgVerify), new PropertyMetadata(20D));



        [Description("是否显示验证码图像")]
        public bool IsShowVerifyImg
        {
            get { return (bool)GetValue(IsShowVerifyImgProperty); }
            set { SetValue(IsShowVerifyImgProperty, value); }
        }

        public static readonly DependencyProperty IsShowVerifyImgProperty =
            DependencyProperty.Register("IsShowVerifyImg", typeof(bool), typeof(RSImgVerify), new PropertyMetadata(false));



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnSlider = this.GetTemplateChild(nameof(this.PART_BtnSlider)) as Thumb;
            this.PART_BtnSliderHost = this.GetTemplateChild(nameof(this.PART_BtnSliderHost)) as Canvas;
            this.PART_BtnImgSlider = this.GetTemplateChild(nameof(this.PART_BtnImgSlider)) as Thumb;
            this.PART_BtnImgSliderHost = this.GetTemplateChild(nameof(this.PART_BtnImgSliderHost)) as Border;

            if (this.PART_BtnSlider != null)
            {
                this.PART_BtnSlider.DragDelta += PART_BtnSlider_DragDelta;
            }

            if (this.PART_BtnImgSlider != null)
            {
                this.PART_BtnImgSlider.DragDelta += PART_BtnImgSlider_DragDelta;
            }
        }

        private void PART_BtnImgSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //获取拖拽按钮的容器的宽度和高度
            var imgHostWidth = this.PART_BtnImgSliderHost.ActualWidth;
            var imgHostHeight = this.PART_BtnImgSliderHost.ActualHeight;

            //获取拖拽按钮的宽度和高度
            var imgBtnWidth = this.PART_BtnImgSlider.ActualWidth;
            var imgBtnHeight = this.PART_BtnImgSlider.ActualHeight;

            //获取容器的CanvasLeft和Top
            var hostCanvasLeft = Canvas.GetLeft(this.PART_BtnImgSliderHost);
            var hostCanvasTop = Canvas.GetTop(this.PART_BtnImgSliderHost);

            //计算拖拽按钮在Canvas容器里左上角横坐标Left值
            var imgBtnCanvasLeft = Canvas.GetLeft(this.PART_BtnImgSlider);
            var imgBtnCanvasTop = Canvas.GetTop(this.PART_BtnImgSlider);

            imgBtnCanvasLeft = imgBtnCanvasLeft + e.HorizontalChange;
            imgBtnCanvasTop = imgBtnCanvasTop + e.VerticalChange;


            //这里就是限制Left最小和最大值
            var moveMaxWidth = imgHostWidth - imgBtnWidth;
            var moveMaxHeight = hostCanvasTop + imgHostHeight - imgBtnHeight;
            imgBtnCanvasLeft = Math.Max(hostCanvasLeft, imgBtnCanvasLeft);
            imgBtnCanvasLeft = Math.Min(moveMaxWidth, imgBtnCanvasLeft);

            imgBtnCanvasTop = Math.Max(hostCanvasTop, imgBtnCanvasTop);
            imgBtnCanvasTop = Math.Min(moveMaxHeight, imgBtnCanvasTop);
            //这里设置值
            Canvas.SetLeft(this.PART_BtnImgSlider, imgBtnCanvasLeft);
            Canvas.SetTop(this.PART_BtnImgSlider, imgBtnCanvasTop);

            ////这里计算拖拽按钮移动时背景色的宽度
            //this.SliderMaskWidth = canvasLeft;
        }

        public event Func<Task<OperateResult<ImgVerifyModel>>> InitVerifyControlAsync;

        private async void PART_BtnSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.PART_BtnSliderHost == null)
            {
                return;
            }

            //获取拖拽按钮的容器的宽度和高度
            var hostWidth = this.PART_BtnSliderHost.ActualWidth;
            var hostHeight = this.PART_BtnSliderHost.ActualHeight;

            var imgHostWidth = this.PART_BtnImgSliderHost.ActualWidth;
            var imgHostHeight = this.PART_BtnImgSliderHost.ActualHeight;

            //获取拖拽按钮的宽度和高度
            var btnWidth = this.PART_BtnSlider.ActualWidth;
            var btnHeight = this.PART_BtnSlider.ActualHeight;
            //计算拖拽按钮在Canvas容器里左上角横坐标Left值
            var canvasLeft = Canvas.GetLeft(this.PART_BtnSlider);
            canvasLeft = canvasLeft + e.HorizontalChange;
            //这里就是限制Left最小和最大值
            var moveMaxWidth = hostWidth - btnWidth;
            canvasLeft = Math.Max(0, canvasLeft);
            canvasLeft = Math.Min(moveMaxWidth, canvasLeft);
            //这里设置值
            Canvas.SetLeft(this.PART_BtnSlider, canvasLeft);

            //这里计算拖拽按钮移动时背景色的宽度
            this.SliderMaskWidth = canvasLeft;

            //计算拖拽百分比

            var movePercent = (canvasLeft + btnWidth) / hostWidth;
            if (movePercent > 0.9 && !this.IsShowVerifyImg)
            {
                this.IsShowVerifyImg = true;
                //这里让用户自己去实现获取验证码的逻辑，只需要告诉结果就可以
                var initVerifyControlResult = await InitVerifyControlAsync?.Invoke();
                if (initVerifyControlResult == null
                    || !initVerifyControlResult.IsSuccess)
                {
                    return;
                }
                var imgVerifyModel = initVerifyControlResult.Data;

                //计算长宽比例
                var widthScale = imgHostWidth / imgVerifyModel.ImgWidth;
                var heightScale = imgHostHeight / imgVerifyModel.ImgHeight;

                this.BtnImgWidth = widthScale * imgVerifyModel.IconWidth;
                this.BtnImgHeight = heightScale * imgVerifyModel.IconHeight;

                //设置图像按钮默认位置
                var imgSliderCanvasLeft = imgVerifyModel.ImgBtnPositionX * widthScale;
                var imgSliderCanvasTop = imgVerifyModel.ImgBtnPositionY * heightScale;


                //获取容器的CanvasLeft和Top
                var hostCanvasLeft = Canvas.GetLeft(this.PART_BtnImgSliderHost);
                var hostCanvasTop = Canvas.GetTop(this.PART_BtnImgSliderHost);

                //这里设置值
                Canvas.SetLeft(this.PART_BtnImgSlider, hostCanvasLeft + imgSliderCanvasLeft);
                Canvas.SetTop(this.PART_BtnImgSlider, hostCanvasTop + imgSliderCanvasTop);

                //如果成功获取 则进行数据渲染
                this.ParsingImgBuffer(imgVerifyModel.ImgBuffer);
            }

            if (movePercent < 0.2 && this.IsShowVerifyImg)
            {
                this.IsShowVerifyImg = false;
            }
        }



        /// <summary>
        /// 解析图片数据并生成两个 ImageSource
        /// </summary>
        public void ParsingImgBuffer(byte[] buffer)
        {

            // 读取前4个字节（小端）作为第一张图片的长度
            int image1Length = BitConverter.ToInt32(buffer, 0);

            // 分割图片数据
            var image1Data = new byte[image1Length];
            Array.Copy(buffer, 8, image1Data, 0, image1Length);

            var image2Length = buffer.Length - 8 - image1Length;
            var image2Data = new byte[image2Length];
            Array.Copy(buffer, 8 + image1Length, image2Data, 0, image2Length);

            // 转换为 ImageSource
            this.ImgVerifyBackground = ByteArrayToImageSource(image1Data);
            this.BtnImgSource = ByteArrayToImageSource(image2Data);

        }

        private ImageSource ByteArrayToImageSource(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze(); // 线程安全
                return bitmap;
            }
        }

    }
}
