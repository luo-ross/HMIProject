using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class UserFormView : UserControl
    {
        private readonly UserViewModel ViewModel;
        public UserFormView(UserViewModel userViewModel)
        {
            InitializeComponent();
            this.DataContext = userViewModel;
            this.ViewModel = userViewModel;
        }
    }
}
