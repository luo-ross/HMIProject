using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp.WpfExtensions;
using RS.Commons.Attributs;
using RS.HMI.Client.Views.Areas;
using RS.Widgets.Controls;
using System.IO;
using System.Windows;
using System.Windows.Documents;


namespace RS.HMI.Client.Views
{
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class HomeView : RSWindow
    {
        private readonly HomeViewModel ViewModel;
        public HomeView(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            this.DataContext = homeViewModel;
            this.ViewModel = homeViewModel;

            // 检查是否启用了DPI Tier 2
            //bool isTier2Enabled = !AppContext.TryGetSwitch("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater", out bool disabled) || !disabled;

            this.Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
      
    }
}
