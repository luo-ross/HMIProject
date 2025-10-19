using RS.HMI.Client.Models;
using RS.Widgets.Models;

namespace RS.HMI.Client.Views
{
    public  class RegisterViewModel : ViewModelBase
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
            get { return remainingTime; }
            set
            {
                this.SetProperty(ref remainingTime, value);

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
                this.SetProperty(ref signUpModel, value);
            }
        }

    }
}
