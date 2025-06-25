using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using RS.Commons.Attributs;
using RS.Widgets.Controls;
using RS.Widgets.Structs;
using RS.Win32API;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace RS.HMI.Client.Views
{

    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public partial class HomeView : RSWindow
    {
        private readonly HomeViewModel ViewModel;

        public HomeView(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            this.DataContext = homeViewModel;
            this.ViewModel = homeViewModel;
            this.SizeChanged += HomeView_SizeChanged;
            this.Loaded += HomeView_Loaded;
        }

      

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HomeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var width = this.PART_ContentHost.ActualWidth;
            //var height = this.PART_ContentHost.ActualHeight;
            //this.PART_ContentHost.Clip = this.GetBorderClipRect(new CornerRadius(10), width, height);
        }
      

        private async void BtnSearch_OnBtnSearchCallBack(string obj)
        {
            //this.NotifyIcon.ShowBalloonTip(5000, "测试", "Hello World", ToolTipIcon.Info);
            //await this.MessageBox.ShowMessageAsync($@"搜索事件触发, 查询条件{this.ViewModel.Test}");
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        
        }

        private void NotifyIcon_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           
        }
    }
}
