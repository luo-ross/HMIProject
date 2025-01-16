using RS.HMIClient.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIClient.Views.Logoin
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
        /// 注册提提
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
