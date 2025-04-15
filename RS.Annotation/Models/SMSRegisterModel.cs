using RS.Widgets.Models;
using System.ComponentModel.DataAnnotations;

namespace RS.Annotation.Models
{
    /// <summary>
    /// 短信注册类
    /// </summary>
    public class SMSRegisterModel : NotifyBase
    {

        private string phone;
        /// <summary>
        /// 电话号码
        /// </summary>
        [Required(ErrorMessage = "电话号码不能为空")]
        public string Phone
        {
            get { return phone; }
            set
            {
                OnPropertyChanged(ref phone, value);
                ValidProperty(value);
            }
        }

        private string countryCode;
        /// <summary>
        /// 国家代码
        /// </summary>
        [Required(ErrorMessage = "国家代码不能为空")]
        public string CountryCode
        {
            get { return countryCode; }
            set
            {
                OnPropertyChanged(ref countryCode, value);
                ValidProperty(value);
            }
        }



        private string verify;
        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "验证码不能为空")]
        public string Verify
        {
            get { return verify; }
            set
            {
                OnPropertyChanged(ref verify, value);
                ValidProperty(value);
            }
        }
    }
}
