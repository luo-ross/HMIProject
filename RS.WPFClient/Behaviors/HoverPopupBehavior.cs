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
        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register(nameof(Popup), typeof(Popup), typeof(HoverPopupBehavior), new PropertyMetadata(null));

        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty OpenDelayProperty =
            DependencyProperty.Register(nameof(OpenDelay), typeof(int), typeof(HoverPopupBehavior), new PropertyMetadata(500));

        /// <summary>
        /// 开启延迟（毫秒），默认为 500ms
        /// </summary>
        public int OpenDelay
        {
            get { return (int)GetValue(OpenDelayProperty); }
            set { SetValue(OpenDelayProperty, value); }
        }

        private CancellationTokenSource OpenCts;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= OnMouseEnter;
            AssociatedObject.MouseLeave -= OnMouseLeave;
            CancelOpen();
        }

        private async void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (Popup != null)
            {
                if (Popup.IsOpen) return;

                CancelOpen();
                OpenCts = new CancellationTokenSource();
                
                try 
                {
                    await Task.Delay(OpenDelay, OpenCts.Token);
                    
                    if (Popup != null && AssociatedObject.IsMouseOver)
                    {
                        Popup.IsOpen = true;
                        // 确保 Popup 内部也能感知鼠标事件以保持开启
                        if (Popup.Child is FrameworkElement content)
                        {
                            content.MouseEnter -= OnPopupMouseEnter;
                            content.MouseEnter += OnPopupMouseEnter;
                            content.MouseLeave -= OnPopupMouseLeave;
                            content.MouseLeave += OnPopupMouseLeave;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // 正常取消
                }
            }
        }

        private async void OnMouseLeave(object sender, MouseEventArgs e)
        {
            CancelOpen();
            await DelayCheckClose();
        }

        private void CancelOpen()
        {
            OpenCts?.Cancel();
            OpenCts?.Dispose();
            OpenCts = null;
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
            // 给用户 200 毫秒的时间移动鼠标
            await Task.Delay(200);

            if (Popup == null)
            {
                return;
            }

            // 只有当鼠标既不在触发元素上，也不在 Popup 内部时，才关闭
            if (!AssociatedObject.IsMouseOver && !(Popup.Child?.IsMouseOver ?? false))
            {
                Popup.IsOpen = false;
            }
        }
    }
}
