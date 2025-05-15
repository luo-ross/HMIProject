using IdGen;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using RS.Widgets.Models;
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
    public partial class UserView : RSUserControl, IRSLoading
    {
        public UserView()
        {
            InitializeComponent();
        }

    }
}
