using RS.HMI.Client.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Logoin
{
    public class LoginViewModel : NotifyBase
    {
        private LoginModel loginModel;
        /// <summary>
        /// 登录实体
        /// </summary>
        public LoginModel LoginModel
        {
            get
            {
                if (loginModel == null)
                {
                    loginModel = new LoginModel();
                }
                return loginModel;
            }
            set
            {
                this.OnPropertyChanged(ref loginModel, value);
            }
        }



        private SignUpModel signUpModel;
        /// <summary>
        /// 注册
        /// </summary>
        public SignUpModel SignUpModel
        {
            get
            {
                if (signUpModel == null)
                {
                    signUpModel = new SignUpModel();
                }
                return signUpModel;
            }
            set
            {
                this.OnPropertyChanged(ref signUpModel, value);
            }
        }


        
       

    }
}
