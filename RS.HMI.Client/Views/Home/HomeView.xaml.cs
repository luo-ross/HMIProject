using IdGen;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using RS.Commons.Attributs;
using RS.Widgets.Adorners;
using RS.Widgets.Controls;
using RS.Widgets.Structs;
using RS.Win32API;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Navigation;

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
            this.Closing += HomeView_Closing;
        }


        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
        
        }

        private void HomeView_Closing(object? sender, CancelEventArgs e)
        {
        }


        private void HomeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var width = this.PART_ContentHost.ActualWidth;
            //var height = this.PART_ContentHost.ActualHeight;
            //this.PART_ContentHost.Clip = this.GetBorderClipRect(new CornerRadius(10), width, height);
        }

        private void Test_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
          var sdf=  (FrameworkElement)sender;
            var adornerLayer = AdornerLayer.GetAdornerLayer(sdf);
            var rsAdorner = new RSDragAdorner(sdf);
            adornerLayer.Add(rsAdorner);
        }
    }
}
