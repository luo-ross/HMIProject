using RS.Widgets.Models;
using RS.Annotation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.Views.Home
{
   public class HomeViewModel : ViewModelBase
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
                this.SetProperty(ref version, value);
            }
        }
    }
}
