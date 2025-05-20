using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public partial class ModelBase : NotifyBase
    {
        /// <summary>
        /// 正在加载中
        /// </summary>
        [ObservableProperty]
        private bool isLoading;
    
     

        [RelayCommand]
        public virtual async Task Submit(object obj)
        {
         
        }

        [RelayCommand]
        public virtual async Task Update(object obj)
        {

        }


      
    }
}
