using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets;
using RS.Widgets.Adorners;
using RS.Widgets.Controls;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        }
    }
}
