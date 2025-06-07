using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Controls;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
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
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

        }

        private void HomeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = this.PART_ContentHost.ActualWidth;
            var height = this.PART_ContentHost.ActualHeight;
            this.PART_ContentHost.Clip = this.GetBorderClipRect(new CornerRadius(10), width, height);
        }

        private unsafe void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BtnSearch_OnBtnSearchCallBack(string obj)
        {
            //await this.MessageBox.ShowMessageAsync($@"搜索事件触发, 查询条件{this.ViewModel.Test}");
        }


    }
}
