using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.ShowAsync("这是自带点击事件");
        }
    }
}
