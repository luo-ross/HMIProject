using RS.Widgets.Models;
using RS.WPFApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFApp.Views.Home
{
   public class HomeViewModel : ModelBase
    {

        private string? version;
        /// <summary>
        /// 获取当前软件版本
        /// </summary>
        public string? Version
        {
            get
            {
                return version;
            }
            set
            {
                OnPropertyChanged(ref version, value);
            }
        }
    }
}
