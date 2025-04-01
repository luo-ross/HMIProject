using RS.Annotation.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.Views
{
    public class LoginViewModel : ModelBase
    {
        private PasswordLoginModel passwordLoginModel;
        /// <summary>
        /// 密码登录实体类
        /// </summary>
        public PasswordLoginModel PasswordLoginModel
        {
            get
            {
                if (passwordLoginModel==null)
                {
                    passwordLoginModel = new PasswordLoginModel();
                }
                return passwordLoginModel;
            }
            set
            {
                OnPropertyChanged(ref passwordLoginModel, value);
            }
        }


        private SMSRegisterModel smsRegisterModel;
        /// <summary>
        /// 短信登录实体类
        /// </summary>
        public SMSRegisterModel SMSRegisterModel
        {
            get
            {
                if (smsRegisterModel == null)
                {
                    smsRegisterModel = new SMSRegisterModel();
                }
                return smsRegisterModel;
            }
            set
            {
                OnPropertyChanged(ref smsRegisterModel, value);
            }
        }
    }
}
