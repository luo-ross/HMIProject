using RS.Commons;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSLoading : ContentControl, ILoading
    {
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


        public async Task<OperateResult> InvokeAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig, CancellationToken cancellationToken)
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
                    return await func?.Invoke(cancellationToken);
                }
                catch (Exception ex)
                {
                    return OperateResult.CreateFailResult(ex.Message);
                }
                finally
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Visibility = Visibility.Collapsed;
                    });
                }
            }, cancellationToken);
        }


        public async Task<OperateResult<T>> InvokeAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig, CancellationToken cancellationToken)
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
                    return await func?.Invoke(cancellationToken);
                }
                catch (Exception ex)
                {
                    return OperateResult.CreateFailResult<T>(ex.Message);
                }
                finally
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Visibility = Visibility.Collapsed;
                    });
                }
            }, cancellationToken);
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void HideLoading()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Visibility=Visibility.Collapsed;
            });
        }

        public void ShowLoading(LoadingConfig loadingConfig)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.LoadingConfig = loadingConfig;
                this.Visibility = Visibility.Visible;
            });
        }
    }
}
