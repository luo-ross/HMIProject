using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.UserModel;
using Org.BouncyCastle.Asn1.Ocsp;
using RS.Commons.Attributs;
using RS.Commons.Helper;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using RS.WPFClient.IServices;
using RS.WPFClient.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

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
