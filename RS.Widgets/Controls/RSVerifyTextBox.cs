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
    [TemplatePart(Name = nameof(PART_BtnVerify), Type = typeof(Button))]
    public class RSVerifyTextBox : ContentControl
    {

        private Button PART_BtnVerify;

        private DispatcherTimer ExpireTimeDispatcherTimer;
        static RSVerifyTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSVerifyTextBox), new FrameworkPropertyMetadata(typeof(RSVerifyTextBox)));
        }


        /// <summary>
        /// 用户输入的验证码
        /// </summary>
        public string Verify
        {
            get { return (string)GetValue(VerifyProperty); }
            set { SetValue(VerifyProperty, value); }
        }

        public static readonly DependencyProperty VerifyProperty =
            DependencyProperty.Register("Verify", typeof(string), typeof(RSVerifyTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public VerifyModel VerifyResultModel
        {
            get { return (VerifyModel)GetValue(VerifyResultModelProperty); }
            set { SetValue(VerifyResultModelProperty, value); }
        }

       
        public static readonly DependencyProperty VerifyResultModelProperty =
            DependencyProperty.Register("VerifyResultModel", typeof(VerifyModel), typeof(RSVerifyTextBox), new PropertyMetadata(null, OnVerifyResultModelPropertyChanged));

        private static void OnVerifyResultModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rSVerifyTextBox = d as RSVerifyTextBox;
        
            if (rSVerifyTextBox.VerifyResultModel != null && rSVerifyTextBox.VerifyResultModel.IsSuccess)
            {

                rSVerifyTextBox.ExpireTimeDispatcherTimer = new DispatcherTimer();
                rSVerifyTextBox.ExpireTimeDispatcherTimer.Tick += rSVerifyTextBox.ExpireTimeDispatcherTimer_Tick;
                rSVerifyTextBox.ExpireTimeDispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
                rSVerifyTextBox.ExpireTimeDispatcherTimer.Start();
                rSVerifyTextBox. ExpireTimeDispatcherTimer_Tick(null, new EventArgs());
                rSVerifyTextBox.IsVerifySuccess = rSVerifyTextBox.VerifyResultModel.IsSuccess;
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
            DependencyProperty.Register("ExpireTimeDes", typeof(string), typeof(RSVerifyTextBox), new PropertyMetadata(null));



        /// <summary>
        /// 是否成功获取验证码
        /// </summary>
        public bool IsVerifySuccess
        {
            get { return (bool)GetValue(IsVerifySuccessProperty); }
            set { SetValue(IsVerifySuccessProperty, value); }
        }

        public static readonly DependencyProperty IsVerifySuccessProperty =
            DependencyProperty.Register("IsVerifySuccess", typeof(bool), typeof(RSVerifyTextBox), new PropertyMetadata(false));


        public delegate Task<VerifyModel> GetVerifyClickEventHandler();
        /// <summary>
        /// 获取验证码点击事件
        /// </summary>
        public event GetVerifyClickEventHandler GetVerifyClick;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnVerify = GetTemplateChild(nameof(PART_BtnVerify)) as Button;

            if (this.PART_BtnVerify is not null)
            {
                this.PART_BtnVerify.Click -= PART_BtnVerify_Click;
                this.PART_BtnVerify.Click += PART_BtnVerify_Click;
            }
        }

        private async void PART_BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            this.VerifyResultModel =await GetVerifyClick?.Invoke();
        }

        private void ExpireTimeDispatcherTimer_Tick(object? sender, EventArgs e)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(this.VerifyResultModel.ExpireTime);
            var expireTime = dateTimeOffset.LocalDateTime;

            if (DateTime.Now >= expireTime.AddSeconds(-1))
            {
                this.ExpireTimeDispatcherTimer.Stop();
                this.ExpireTimeDispatcherTimer = null;
                this.IsVerifySuccess = false;
                return;
            }
            //获取时间差
            var timeDiff = expireTime - DateTime.Now;
            this.ExpireTimeDes = $"{(int)timeDiff.TotalSeconds}秒后获取";
        }
    }
}
