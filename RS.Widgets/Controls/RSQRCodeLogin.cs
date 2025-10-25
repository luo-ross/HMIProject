using RS.Commons.Enums;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZXing;
using ZXing.QrCode;


namespace RS.Widgets.Controls
{
    [TemplatePart(Name = nameof(PART_BtnReGetQRCode), Type = typeof(Button))]
    public class RSQRCodeLogin : ContentControl
    {
        private class QRCodeLoginTask : IDisposable
        {
            private DispatcherTimer QueryQRCodeLoginStatusDispatcherTimer;
            private QRCodeLoginResultModel LoginQRCodeResult;
            private RSQRCodeLogin RSQRCodeLogin;
            public Action<QRCodeLoginResultModel> CancelQRCodeLoginCallBack;
            public Func<Task<QRCodeLoginResultModel>> GetLoginQRCodeCallBack;
            public Action<QRCodeLoginResultModel> QRCodeAuthLoginSuccessCallBack;
            public Func<Task<QRCodeLoginStatusModel>> QueryQRCodeLoginStatusCallBack;
            private bool IsEndQRCodeLogin = false;


            public QRCodeLoginTask(RSQRCodeLogin rSQRCodeLogin)
            {
                RSQRCodeLogin = rSQRCodeLogin;
            }

            ~QRCodeLoginTask()
            {
                Dispose();
            }

            public async void BeginQRCodeLogin()
            {

                RSQRCodeLogin.QRCodeLoginStatus = QRCodeLoginStatusEnum.BeginGetQRCode;
                this.GenerateQRCodeImgSource(" ", RSQRCodeLogin.QRCodeWidth, RSQRCodeLogin.QRCodeHeight);

                if (GetLoginQRCodeCallBack == null)
                {
                    return;
                }

                this.LoginQRCodeResult = await GetLoginQRCodeCallBack?.Invoke();
                if (IsEndQRCodeLogin)
                {
                    return;
                }

                if (this.LoginQRCodeResult != null && this.LoginQRCodeResult.IsSuccess)
                {

                    this.GenerateQRCodeImgSource(this.LoginQRCodeResult.QRCodeContent, RSQRCodeLogin.QRCodeWidth, RSQRCodeLogin.QRCodeHeight);
                    if (!IsEndQRCodeLogin)
                    {
                        this.QueryQRCodeLoginStatusDispatcherTimer = new DispatcherTimer();
                        this.QueryQRCodeLoginStatusDispatcherTimer.Tick += QueryQRCodeLoginStatusDispatcherTimer_Tick; ;
                        this.QueryQRCodeLoginStatusDispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
                        this.QueryQRCodeLoginStatusDispatcherTimer.Start();
                    }
                    if (!IsEndQRCodeLogin)
                    {
                        QueryQRCodeLoginStatusDispatcherTimer_Tick(null, new EventArgs());
                    }
                }
                else
                {
                    if (!IsEndQRCodeLogin)
                    {
                        RSQRCodeLogin.QRCodeLoginStatus = QRCodeLoginStatusEnum.QRCodeLoginTimeOut;
                    }
                }
            }


            public async void EndQRCodeLogin()
            {
                IsEndQRCodeLogin = true;
                QueryQRCodeLoginStatusDispatcherTimer?.Stop();
                QueryQRCodeLoginStatusDispatcherTimer = null;
            }


            private async void QueryQRCodeLoginStatusDispatcherTimer_Tick(object? sender, EventArgs e)
            {
                if (this.QueryQRCodeLoginStatusDispatcherTimer == null)
                {
                    return;
                }
                if (this.LoginQRCodeResult == null)
                {
                    this.QueryQRCodeLoginStatusDispatcherTimer.Stop();
                    return;
                }

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(this.LoginQRCodeResult.ExpireTime);
                var expireTime = dateTimeOffset.LocalDateTime;

                if (DateTime.Now >= expireTime.AddSeconds(-1))
                {
                    this.QueryQRCodeLoginStatusDispatcherTimer.Stop();
                    if (!IsEndQRCodeLogin)
                    {
                        RSQRCodeLogin.QRCodeLoginStatus = QRCodeLoginStatusEnum.QRCodeLoginTimeOut;
                    }
                    return;
                }

                if (!IsEndQRCodeLogin)
                {
                    var qRCodeLoginStatusModel = await QueryQRCodeLoginStatusCallBack?.Invoke();
                    if (qRCodeLoginStatusModel != null && qRCodeLoginStatusModel.IsSuccess)
                    {
                        if (!IsEndQRCodeLogin)
                        {
                            RSQRCodeLogin.QRCodeLoginStatus = qRCodeLoginStatusModel.QRCodeLoginStatus;
                        }

                        switch (RSQRCodeLogin.QRCodeLoginStatus)
                        {
                            case QRCodeLoginStatusEnum.BeginGetQRCode:
                                break;
                            case QRCodeLoginStatusEnum.WaitScanQRCode:
                                break;
                            case QRCodeLoginStatusEnum.ScanQRCodeSuccess:
                                break;
                            case QRCodeLoginStatusEnum.QRCodeAuthLogin:
                                this.QueryQRCodeLoginStatusDispatcherTimer.Stop();
                                if (!IsEndQRCodeLogin)
                                {
                                    QRCodeAuthLoginSuccessCallBack?.Invoke(this.LoginQRCodeResult);
                                }

                                break;
                            case QRCodeLoginStatusEnum.CancelQRCodeLogin:
                                this.QueryQRCodeLoginStatusDispatcherTimer.Stop();
                                if (!IsEndQRCodeLogin)
                                {
                                    CancelQRCodeLoginCallBack?.Invoke(this.LoginQRCodeResult);
                                }
                                break;
                            case QRCodeLoginStatusEnum.QRCodeLoginTimeOut:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!IsEndQRCodeLogin)
                        {
                            RSQRCodeLogin.QRCodeLoginStatus = QRCodeLoginStatusEnum.QRCodeLoginTimeOut;
                        }
                    }
                    if (IsEndQRCodeLogin)
                    {
                        this.QueryQRCodeLoginStatusDispatcherTimer.Stop();
                    }
                }
            }

            private void GenerateQRCodeImgSource(string qRCodeContent, double qRCodeWidth, double qRCodeHeight)
            {
                BarcodeWriterPixelData barcodeWriterPixelData = new BarcodeWriterPixelData()
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions()
                    {
                        Height = (int)qRCodeHeight,
                        Width = (int)qRCodeWidth,
                        Margin = 0
                    }
                };

                var pixelData = barcodeWriterPixelData.Write(qRCodeContent);

                double dpiX = 96D; double dpiY = 96D;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiX = graphics.DpiX;
                    dpiY = graphics.DpiY;
                }
                WriteableBitmap writeableBitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, dpiX, dpiY, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelData.Width, pixelData.Height), pixelData.Pixels, pixelData.Width * sizeof(int), 0);

                if (!this.IsEndQRCodeLogin)
                {
                    RSQRCodeLogin.QRCodeImgSource = writeableBitmap;
                }
            }

            public void Dispose()
            {
                QueryQRCodeLoginStatusDispatcherTimer?.Stop();
                //QueryQRCodeLoginStatusDispatcherTimer = null;
                //LoginQRCodeResult = null;
                GC.SuppressFinalize(this);
            }
        }


        private Button PART_BtnReGetQRCode;
        private QRCodeLoginTask CurrentQRCodeLoginTask;

        /// <summary>
        /// 获取登录二维码
        /// </summary>
        public Func<Task<QRCodeLoginResultModel>> GetLoginQRCodeCallBack;

        /// <summary>
        /// 查询二维码登录状态
        /// </summary>
        public Func<Task<QRCodeLoginStatusModel>> QueryQRCodeLoginStatusCallBack;

        /// <summary>
        /// 二维码授权登录成功
        /// </summary>
        public Action<QRCodeLoginResultModel> QRCodeAuthLoginSuccessCallBack;

        /// <summary>
        /// 取消二维码登录
        /// </summary>
        public Action<QRCodeLoginResultModel> CancelQRCodeLoginCallBack;

        static RSQRCodeLogin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSQRCodeLogin), new FrameworkPropertyMetadata(typeof(RSQRCodeLogin)));
        }

        public RSQRCodeLogin()
        {

        }


        public Action<QRCodeLoginResultModel> CancelQRCodeLoginCommand
        {
            get { return (Action<QRCodeLoginResultModel>)GetValue(CancelQRCodeLoginCommandProperty); }
            set { SetValue(CancelQRCodeLoginCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelQRCodeLoginCommandProperty =
            DependencyProperty.Register("CancelQRCodeLoginCommand", typeof(Action<QRCodeLoginResultModel>), typeof(RSQRCodeLogin), new PropertyMetadata(null));




        public Action<QRCodeLoginResultModel> QRCodeAuthLoginSuccessCommand
        {
            get { return (Action<QRCodeLoginResultModel>)GetValue(QRCodeAuthLoginSuccessCommandProperty); }
            set { SetValue(QRCodeAuthLoginSuccessCommandProperty, value); }
        }

        public static readonly DependencyProperty QRCodeAuthLoginSuccessCommandProperty =
            DependencyProperty.Register("QRCodeAuthLoginSuccessCommand", typeof(Action<QRCodeLoginResultModel>), typeof(RSQRCodeLogin), new PropertyMetadata(null));


        public Func<Task<QRCodeLoginStatusModel>> QueryQRCodeLoginStatusCommand
        {
            get { return (Func<Task<QRCodeLoginStatusModel>>)GetValue(QueryQRCodeLoginStatusCommandProperty); }
            set { SetValue(QueryQRCodeLoginStatusCommandProperty, value); }
        }

        public static readonly DependencyProperty QueryQRCodeLoginStatusCommandProperty =
            DependencyProperty.Register("QueryQRCodeLoginStatusCommand", typeof(Func<Task<QRCodeLoginStatusModel>>), typeof(RSQRCodeLogin), new PropertyMetadata(null));



        public Func<Task<QRCodeLoginResultModel>> GetLoginQRCodeCommand
        {
            get { return (Func<Task<QRCodeLoginResultModel>>)GetValue(GetLoginQRCodeCommandProperty); }
            set { SetValue(GetLoginQRCodeCommandProperty, value); }
        }

        public static readonly DependencyProperty GetLoginQRCodeCommandProperty =
            DependencyProperty.Register("GetLoginQRCodeCommand", typeof(Func<Task<QRCodeLoginResultModel>>), typeof(RSQRCodeLogin), new PropertyMetadata(null));





        private void BeginQRCodeLoginTrigger()
        {
            if (CurrentQRCodeLoginTask != null)
            {
                CurrentQRCodeLoginTask.EndQRCodeLogin();
            }
            using (QRCodeLoginTask qRCodeLoginTask = new QRCodeLoginTask(this))
            {
                qRCodeLoginTask.CancelQRCodeLoginCallBack = this.CancelQRCodeLoginCommand ?? this.CancelQRCodeLoginCallBack;
                qRCodeLoginTask.GetLoginQRCodeCallBack = this.GetLoginQRCodeCommand ?? this.GetLoginQRCodeCallBack;
                qRCodeLoginTask.QRCodeAuthLoginSuccessCallBack = this.QRCodeAuthLoginSuccessCommand ?? this.QRCodeAuthLoginSuccessCallBack;
                qRCodeLoginTask.QueryQRCodeLoginStatusCallBack = this.QueryQRCodeLoginStatusCommand ?? this.QueryQRCodeLoginStatusCallBack;
                CurrentQRCodeLoginTask = qRCodeLoginTask;
                qRCodeLoginTask.BeginQRCodeLogin();
            }
        }

        private void EndQRCodeLoginTrigger()
        {
            if (CurrentQRCodeLoginTask != null)
            {
                CurrentQRCodeLoginTask.EndQRCodeLogin();
            }
            CurrentQRCodeLoginTask = null;
        }


        /// <summary>
        /// 二维码宽度
        /// </summary>
        public double QRCodeWidth
        {
            get { return (double)GetValue(QRCodeWidthProperty); }
            set { SetValue(QRCodeWidthProperty, value); }
        }


        public static readonly DependencyProperty QRCodeWidthProperty =
            DependencyProperty.Register("QRCodeWidth", typeof(double), typeof(RSQRCodeLogin), new PropertyMetadata(150D));

        /// <summary>
        /// 二维码高度
        /// </summary>
        public double QRCodeHeight
        {
            get { return (double)GetValue(QRCodeHeightProperty); }
            set { SetValue(QRCodeHeightProperty, value); }
        }


        public static readonly DependencyProperty QRCodeHeightProperty =
            DependencyProperty.Register("QRCodeHeight", typeof(double), typeof(RSQRCodeLogin), new PropertyMetadata(150D));


        /// <summary>
        /// 二维码图片资源
        /// </summary>
        public ImageSource QRCodeImgSource
        {
            get { return (ImageSource)GetValue(QRCodeImgSourceProperty); }
            set { SetValue(QRCodeImgSourceProperty, value); }
        }
        public static readonly DependencyProperty QRCodeImgSourceProperty =
            DependencyProperty.Register("QRCodeImgSource", typeof(ImageSource), typeof(RSQRCodeLogin), new PropertyMetadata(null));


        /// <summary>
        /// 是否获取登录二维码
        /// </summary>
        public bool IsQRCodeLogin
        {
            get { return (bool)GetValue(IsQRCodeLoginProperty); }
            set { SetValue(IsQRCodeLoginProperty, value); }
        }

        public static readonly DependencyProperty IsQRCodeLoginProperty =
            DependencyProperty.Register("IsQRCodeLogin", typeof(bool), typeof(RSQRCodeLogin), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsQRCodeLoginPropertyChanged));

        private static void OnIsQRCodeLoginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RSQRCodeLogin rSQRCodeLogin)
            {
                if (rSQRCodeLogin.IsQRCodeLogin)
                {
                    rSQRCodeLogin.BeginQRCodeLoginTrigger();
                }
                else
                {
                    rSQRCodeLogin.EndQRCodeLoginTrigger();

                }
            }
        }


        /// <summary>
        /// 二维码登录状态
        /// </summary>
        public QRCodeLoginStatusEnum QRCodeLoginStatus
        {
            get { return (QRCodeLoginStatusEnum)GetValue(QRCodeLoginStatusProperty); }
            set { SetValue(QRCodeLoginStatusProperty, value); }
        }

        public static readonly DependencyProperty QRCodeLoginStatusProperty =
            DependencyProperty.Register("QRCodeLoginStatus", typeof(QRCodeLoginStatusEnum), typeof(RSQRCodeLogin), new PropertyMetadata(QRCodeLoginStatusEnum.BeginGetQRCode));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_BtnReGetQRCode = GetTemplateChild(nameof(PART_BtnReGetQRCode)) as Button;

            if (this.PART_BtnReGetQRCode is not null)
            {
                this.PART_BtnReGetQRCode.Click -= PART_BtnReGetQRCode_Click;
                this.PART_BtnReGetQRCode.Click += PART_BtnReGetQRCode_Click;
            }
        }

        private void PART_BtnReGetQRCode_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentQRCodeLoginTask?.BeginQRCodeLogin();
        }
    }

  
}
