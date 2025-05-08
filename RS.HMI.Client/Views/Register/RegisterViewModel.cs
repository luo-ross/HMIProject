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

namespace RS.HMI.Client.Views
{
    public class RegisterViewModel : NotifyBase
    {
        public RegisterViewModel()
        {
           
        }


        private double remainingTime;
        /// <summary>
        /// 剩余时间
        /// </summary>
        public double RemainingTime
        {
            get
            {
                return remainingTime;
            }
            set
            {
                this.OnPropertyChanged(ref remainingTime, value);
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
