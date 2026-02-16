using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.UserModel;
using Org.BouncyCastle.Asn1.Ocsp;
using RS.Commons.Attributs;
using RS.Commons.Helper;
using RS.Widgets.Controls;
using RS.Win32API.Enums;
using RS.WPFClient.IServices;
using RS.WPFClient.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;


namespace RS.WPFClient.Views
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class HomeView : RSWindow
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}
