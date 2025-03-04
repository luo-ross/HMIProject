using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
namespace RS.Widgets.Controls
{
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public class RSWinInfoBar : Window
    {
        /// <summary>
        /// 这里是线程安全的数据
        /// </summary>
        public ConcurrentBag<InfoBarModel> InfoBarModelDataSource { get; set; }

        public CancellationTokenSource HandleInfoBarCTS { get; set; }

        static RSWinInfoBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWinInfoBar), new FrameworkPropertyMetadata(typeof(RSWinInfoBar)));
        }

        public RSWinInfoBar()
        {
            this.InfoBarModelDataSource = new ConcurrentBag<InfoBarModel>();
            this.HandleInfoBarCTS = new CancellationTokenSource();
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Topmost = true;
            this.Loaded += RSWinInfoBar_Loaded;
            this.ShowInTaskbar = false;
            this.InfoBarModelList = new ObservableCollection<InfoBarModel>();
            this.Closing += RSWinInfoBar_Closing;
            this.Show();
        }

        private void RSWinInfoBar_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            this.HandleInfoBarCTS?.Cancel();


        }

        private void HandleInfoBarAsync()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (!this.HandleInfoBarCTS.Token.IsCancellationRequested)
                    {

                        if (InfoBarModelDataSource.Count > 0 && InfoBarModelDataSource.TryTake(out InfoBarModel infoBarModel))
                        {
                            //添加消息
                            this.Dispatcher.Invoke(() =>
                            {
                                this.InfoBarModelList.Add(infoBarModel);
                            });
                        }
                        else
                        {
                            await Task.Delay(100, this.HandleInfoBarCTS.Token);
                        }


                        var infoBarModelList = new List<InfoBarModel>();
                        //添加消息
                        this.Dispatcher.Invoke(() =>
                        {
                            infoBarModelList = this.InfoBarModelList.ToList();
                        });

                        if (infoBarModelList.Count == 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                this.Visibility = Visibility.Collapsed;
                            });
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                this.Visibility = Visibility.Visible;
                            });
                        }

                        //移除消息
                        foreach (var item in infoBarModelList)
                        {
                            if (this.HandleInfoBarCTS.Token.IsCancellationRequested)
                            {
                                break;
                            }
                            if (DateTime.Now.Subtract(item.CreateTime).TotalMilliseconds >= 5000)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    this.InfoBarModelList.Remove(item);
                                });
                            }
                        }


                    }
                }
                catch (TaskCanceledException)
                {

                }
            }, this.HandleInfoBarCTS.Token);
        }

        private void RSWinInfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshWindowSizeAndLocation();
            //初始化线程事件
            this.HandleInfoBarAsync();
        }




        public ObservableCollection<InfoBarModel> InfoBarModelList
        {
            get { return (ObservableCollection<InfoBarModel>)GetValue(InfoBarModelListProperty); }
            private set { SetValue(InfoBarModelListProperty, value); }
        }

        public static readonly DependencyProperty InfoBarModelListProperty =
            DependencyProperty.Register("InfoBarModelList", typeof(ObservableCollection<InfoBarModel>), typeof(RSWinInfoBar), new PropertyMetadata(null));





        private void RefreshWindowSizeAndLocation()
        {
            // 使用 WindowInteropHelper 获取窗口句柄
            var hWnd = new WindowInteropHelper(this).Handle;
            int nWidth = (int)SystemParameters.WorkArea.Width;  // 新的宽度
            int nHeight = (int)SystemParameters.WorkArea.Height; // 新的高度
            //Ross.SetWindowPos(new HWND(hWnd), HWND.Null, (nWidth - 300)/2, 0, 300, nHeight, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
            Ross.SetWindowPos(new HWND(hWnd), HWND.Null, nWidth - 300, 0, 300, nHeight, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
        }

        public void AddMessageAsync(string message, InfoType infoType = InfoType.None)
        {
            var infoBarModel = new InfoBarModel()
            {
                CreateTime = DateTime.Now,
                Message = message,
                InfoType = infoType
            };


            this.InfoBarModelDataSource.Add(infoBarModel);
        }
    }
}
