using RS.HMI.Models.Widgets;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSLoading : ContentControl
    {
        private ProgressBar PART_Loading;
        static RSLoading()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSLoading), new FrameworkPropertyMetadata(typeof(RSLoading)));
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public LoadingConfig LoadingConfig
        {
            get { return (LoadingConfig)GetValue(LoadingConfigProperty); }
            set { SetValue(LoadingConfigProperty, value); }
        }

        public static readonly DependencyProperty LoadingConfigProperty =
            DependencyProperty.Register("LoadingConfig", typeof(LoadingConfig), typeof(RSLoading), new PropertyMetadata(new LoadingConfig(), OnLoadingConfigPropertyChanged));

        private static void OnLoadingConfigPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsLoading = d as RSLoading;

            var loadingConfig = e.NewValue as LoadingConfig;
        }

        public async Task<OperateResult> InvokeLoadingActionAsync(Func<Task<OperateResult>> func, LoadingConfig loadingConfig)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Visibility = Visibility.Visible;
                this.LoadingConfig = loadingConfig == null ? new LoadingConfig() : loadingConfig;
            });
            return await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    return await func?.Invoke();
                }
                catch (Exception ex)
                {
                    return ErrorOperateResult.CreateResult(0, "出现异常了", ex);
                }
                finally
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Visibility = Visibility.Collapsed;
                    });
                }
            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Loading = this.GetTemplateChild(nameof(this.PART_Loading)) as ProgressBar;
        }
    }
}
