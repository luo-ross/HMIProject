using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public  class LoginModel : UserBaseModel
    {

        private bool isRememberPassword;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsRememberPassword
        {
            get { return isRememberPassword; }
            set
            {
                this.SetProperty(ref isRememberPassword,value);
                isRememberPassword = value;
            }
        }
    }
}
