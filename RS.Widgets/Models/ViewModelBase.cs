using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public  class ViewModelBase : NotifyBase
    {
        public ViewModelBase()
        {
            this.DialogKey = Guid.NewGuid().ToString();
        }
   

        private string dialogKey;
        /// <summary>
        /// 需要使用的Dialog主键
        /// </summary>
        public string DialogKey
        {
            get { return dialogKey; }
            set
            {
                dialogKey = value;
                this.SetProperty(ref dialogKey, value);
            }
        }

        public IDialog? Dialog
        {
            get
            {
                return DialogManager.GetDialog(this.DialogKey, false);
            }
        }

    }

}