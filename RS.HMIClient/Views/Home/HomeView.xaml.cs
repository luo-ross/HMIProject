using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RS.BorderWindowDemo.Views.Home
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class HomeView : RSWindow
    {
        public HomeViewModel ViewModel { get; set; }
        public HomeView()
        {
            InitializeComponent();
            this.ViewModel=this.DataContext as HomeViewModel;
            this.SizeChanged += HomeView_SizeChanged;
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
            //await this.MessageBox.ShowAsync($@"搜索事件触发, 查询条件{this.ViewModel.Test}");
        }
    }
}
