using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public partial class ViewModelBase : NotifyBase
    {
        public ViewModelBase()
        {
            this.DialogKey = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 需要使用的Dialog主键
        /// </summary>
        [ObservableProperty]
        private string dialogKey;

        public IDialog? Dialog
        {
            get
            {
                return DialogManager.GetDialog(this.DialogKey, false); 
            }
        }

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