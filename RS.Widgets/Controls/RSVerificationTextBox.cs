using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    [TemplatePart(Name = nameof(PART_BtnVerification), Type = typeof(Button))]
    public class RSVerificationTextBox : ContentControl
    {

        private Button PART_BtnVerification;

        private DispatcherTimer ExpireTimeDispatcherTimer;
        static RSVerificationTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSVerificationTextBox), new FrameworkPropertyMetadata(typeof(RSVerificationTextBox)));
        }


        /// <summary>
        /// 用户输入的验证码
        /// </summary>
        public string Verification
        {
            get { return (string)GetValue(VerificationProperty); }
            set { SetValue(VerificationProperty, value); }
        }

        public static readonly DependencyProperty VerificationProperty =
            DependencyProperty.Register("Verification", typeof(string), typeof(RSVerificationTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public VerificationModel VerificationResultModel
        {
            get { return (VerificationModel)GetValue(VerificationResultModelProperty); }
            set { SetValue(VerificationResultModelProperty, value); }
        }

       
        public static readonly DependencyProperty VerificationResultModelProperty =
            DependencyProperty.Register("VerificationResultModel", typeof(VerificationModel), typeof(RSVerificationTextBox), new PropertyMetadata(null, OnVerificationResultModelPropertyChanged));

        private static void OnVerificationResultModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rSVerificationTextBox = d as RSVerificationTextBox;
        
            if (rSVerificationTextBox.VerificationResultModel != null && rSVerificationTextBox.VerificationResultModel.IsSuccess)
            {

                rSVerificationTextBox.ExpireTimeDispatcherTimer = new DispatcherTimer();
                rSVerificationTextBox.ExpireTimeDispatcherTimer.Tick += rSVerificationTextBox.ExpireTimeDispatcherTimer_Tick;
                rSVerificationTextBox.ExpireTimeDispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
                rSVerificationTextBox.ExpireTimeDispatcherTimer.Start();
                rSVerificationTextBox. ExpireTimeDispatcherTimer_Tick(null, new EventArgs());
                rSVerificationTextBox.IsVerificationSuccess = rSVerificationTextBox.VerificationResultModel.IsSuccess;
            }
        }





        /// <summary>
        /// 超时描述
        /// </summary>
        public string ExpireTimeDes
        {
            get { return (string)GetValue(ExpireTimeDesProperty); }
            set { SetValue(ExpireTimeDesProperty, value); }
        }
        public static readonly DependencyProperty ExpireTimeDesProperty =
            DependencyProperty.Register("ExpireTimeDes", typeof(string), typeof(RSVerificationTextBox), new PropertyMetadata(null));



        /// <summary>
        /// 是否成功获取验证码
        /// </summary>
        public bool IsVerificationSuccess
        {
            get { return (bool)GetValue(IsVerificationSuccessProperty); }
            set { SetValue(IsVerificationSuccessProperty, value); }
        }

        public static readonly DependencyProperty IsVerificationSuccessProperty =
            DependencyProperty.Register("IsVerificationSuccess", typeof(bool), typeof(RSVerificationTextBox), new PropertyMetadata(false));


        public delegate Task<VerificationModel> GetVerificationClickEventHandler();
        /// <summary>
        /// 获取验证码点击事件
        /// </summary>
        public event GetVerificationClickEventHandler GetVerificationClick;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnVerification = GetTemplateChild(nameof(PART_BtnVerification)) as Button;

            if (this.PART_BtnVerification is not null)
            {
                this.PART_BtnVerification.Click -= PART_BtnVerification_Click;
                this.PART_BtnVerification.Click += PART_BtnVerification_Click;
            }
        }

        private async void PART_BtnVerification_Click(object sender, RoutedEventArgs e)
        {
            this.VerificationResultModel =await GetVerificationClick?.Invoke();
        }

        private void ExpireTimeDispatcherTimer_Tick(object? sender, EventArgs e)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(this.VerificationResultModel.ExpireTime);
            var expireTime = dateTimeOffset.LocalDateTime;

            if (DateTime.Now >= expireTime.AddSeconds(-1))
            {
                this.ExpireTimeDispatcherTimer.Stop();
                this.ExpireTimeDispatcherTimer = null;
                this.IsVerificationSuccess = false;
                return;
            }
            //获取时间差
            var timeDiff = expireTime - DateTime.Now;
            this.ExpireTimeDes = $"{(int)timeDiff.TotalSeconds}秒后获取";
        }
    }
}
