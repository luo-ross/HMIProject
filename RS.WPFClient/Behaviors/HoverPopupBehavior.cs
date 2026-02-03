using Microsoft.Xaml.Behaviors;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RS.WPFClient.Behaviors
{
    /// <summary>
    /// 实现悬停显示 Popup 且允许鼠标移入 Popup 进行操作的行为
    /// </summary>
    public class HoverPopupBehavior : Behavior<FrameworkElement>
    {
        private CancellationTokenSource openCts;

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register(
                nameof(Popup),
                typeof(Popup),
                typeof(HoverPopupBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// 要显示的 Popup 控件
        /// </summary>
        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty OpenDelayProperty =
            DependencyProperty.Register(
                nameof(OpenDelay),
                typeof(int),
                typeof(HoverPopupBehavior),
                new PropertyMetadata(500));

        /// <summary>
        /// 开启延迟（毫秒），默认为 500ms
        /// </summary>
        public int OpenDelay
        {
            get { return (int)GetValue(OpenDelayProperty); }
            set { SetValue(OpenDelayProperty, value); }
        }

        public static readonly DependencyProperty CloseDelayProperty =
            DependencyProperty.Register(
                nameof(CloseDelay),
                typeof(int),
                typeof(HoverPopupBehavior),
                new PropertyMetadata(200));

        /// <summary>
        /// 关闭延迟（毫秒），默认为 200ms
        /// </summary>
        public int CloseDelay
        {
            get { return (int)GetValue(CloseDelayProperty); }
            set { SetValue(CloseDelayProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseEnter -= OnMouseEnter;
            AssociatedObject.MouseLeave -= OnMouseLeave;
            CancelOpen();
            base.OnDetaching();
        }

        private async void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (Popup == null || Popup.IsOpen)
            {
                return;
            }

            CancelOpen();
            openCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(OpenDelay, openCts.Token);

                if (Popup != null && AssociatedObject.IsMouseOver)
                {
                    Popup.IsOpen = true;
                    SubscribePopupEvents();
                }
            }
            catch (TaskCanceledException)
            {
                // 正常取消，无需处理
            }
        }

        private async void OnMouseLeave(object sender, MouseEventArgs e)
        {
            CancelOpen();
            await DelayCheckClose();
        }

        private void CancelOpen()
        {
            if (openCts != null)
            {
                openCts.Cancel();
                openCts.Dispose();
                openCts = null;
            }
        }

        private void SubscribePopupEvents()
        {
            if (Popup?.Child is FrameworkElement content)
            {
                content.MouseEnter -= OnPopupMouseEnter;
                content.MouseEnter += OnPopupMouseEnter;
                content.MouseLeave -= OnPopupMouseLeave;
                content.MouseLeave += OnPopupMouseLeave;
            }
        }

        private void OnPopupMouseEnter(object sender, MouseEventArgs e)
        {
            if (Popup != null)
            {
                Popup.IsOpen = true;
            }
        }

        private async void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            await DelayCheckClose();
        }

        private async Task DelayCheckClose()
        {
            await Task.Delay(CloseDelay);

            if (Popup == null)
            {
                return;
            }

            // 只有当鼠标既不在触发元素上，也不在 Popup 内部时，才关闭
            bool isMouseOverTarget = AssociatedObject.IsMouseOver;
            bool isMouseOverPopup = Popup.Child?.IsMouseOver ?? false;

            if (!isMouseOverTarget && !isMouseOverPopup)
            {
                Popup.IsOpen = false;
            }
        }
    }
}
