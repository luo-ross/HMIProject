using RS.WPFApp.Enums;
using RS.WPFApp.Models;
using RS.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFApp.Views
{
    public class RegisterViewModel : ModelBase
    {

        private EmailRegisterModel emailRegisterModel;
        /// <summary>
        /// 邮箱注册
        /// </summary>
        public EmailRegisterModel EmailRegisterModel
        {
            get
            {
                if (emailRegisterModel == null)
                {
                    emailRegisterModel = new Models.EmailRegisterModel();
                }
                return emailRegisterModel;
            }
            set
            {
                OnPropertyChanged(ref emailRegisterModel, value);
            }
        }


        private SMSRegisterModel smsRegisterModel;
        /// <summary>
        /// 短信注册
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


        private RegisterTaskStatus taskStatus;
        /// <summary>
        /// 任务状态
        /// </summary>
        public RegisterTaskStatus TaskStatus
        {
            get { return taskStatus; }
            set
            {
                OnPropertyChanged(ref taskStatus, value);
            }
        }


        private RegisterVerificationModel registerVerificationModel;
        /// <summary>
        /// 注册验证码
        /// </summary>
        public RegisterVerificationModel RegisterVerificationModel
        {
            get { return registerVerificationModel; }
            set
            {
                OnPropertyChanged(ref registerVerificationModel, value);
            }
        }



    }
}
