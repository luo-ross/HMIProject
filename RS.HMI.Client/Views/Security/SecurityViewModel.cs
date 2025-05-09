using RS.HMI.Client.Models;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Media;
using MathNet.Numerics;
using System.Windows.Media.Media3D;
using System.Windows;
using RS.HMI.Client.Controls;
using System.ComponentModel.DataAnnotations;

namespace RS.HMI.Client.Views
{
    public class SecurityViewModel : NotifyBase
    {
        public SecurityViewModel()
        {
           
        }

        private string userName;
        /// <summary>
        /// 邮箱
        /// </summary>
        [MaxLength(30, ErrorMessage = "邮箱长度不能超过30")]
        [Required(ErrorMessage = "邮箱不能为空")]
        [RegularExpression("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$", ErrorMessage = "用户名格式不正确")]
        public string UserName
        {
            get { return userName; }
            set
            {
                this.OnPropertyChanged(ref userName, value);
                this.ValidProperty(value);
            }
        }
    }
}
