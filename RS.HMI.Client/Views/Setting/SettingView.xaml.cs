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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Controls;

namespace RS.HMI.Client.Views.Setting
{
    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public partial class SettingView : RSDialog
    {
        public SettingView()
        {
            InitializeComponent();
        }
    }
}
