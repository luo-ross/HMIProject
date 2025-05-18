using RS.Annotation.Enums;
using RS.Annotation.Models;
using RS.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.Views
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
                this.SetProperty(ref emailRegisterModel, value);
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
                this.SetProperty(ref smsRegisterModel, value);
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
                this.SetProperty(ref taskStatus, value);
            }
        }


        private RegisterVerifyModel registerVerifyModel;
        /// <summary>
        /// 注册验证码
        /// </summary>
        public RegisterVerifyModel RegisterVerifyModel
        {
            get { return registerVerifyModel; }
            set
            {
                this.SetProperty(ref registerVerifyModel, value);
            }
        }



    }
}
