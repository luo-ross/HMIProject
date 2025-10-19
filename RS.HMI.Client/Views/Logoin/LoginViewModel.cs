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



        private ObservableCollection<Advertisement> advertisementLink;
        /// <summary>
        /// 广告信息
        /// </summary>
        public ObservableCollection<Advertisement> AdvertisementList
        {
            get
            {
                if (advertisementLink == null)
                {
                    advertisementLink = new ObservableCollection<Advertisement>();
                }
                return advertisementLink;
            }
            set
            {
                this.OnPropertyChanged(ref advertisementLink, value);
            }
        }




        private ObservableCollection<CarouselSlider> carouselSliderList;
        /// <summary>
        /// 轮播数据集
        /// </summary>
        public ObservableCollection<CarouselSlider> CarouselSliderList
        {
            get
            {
                if (carouselSliderList == null)
                {
                    carouselSliderList = new ObservableCollection<CarouselSlider>();

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Name = "SwiperSlider1",
                        Caption = "native".ToUpper(),
                        Background = "#1b7402",
                        Description = "The most popular yachting destination",
                        ImageSource= "/Assets/img1.jpg",
                        Location= "Whitsunday Islands,Australia ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Name = "SwiperSlider2",
                        Caption = "domestic".ToUpper(),
                        Background = "#62667f",
                        Description = "Enjoy the exotic of sunny Hawaii",
                        ImageSource = "/Assets/img2.jpg",
                        Location = "Maui,Hawaii ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Name = "SwiperSlider3",
                        Caption = "subtropical".ToUpper(),
                        Background = "#087ac4",
                        Description = "The Island of Eternal Spring",
                        ImageSource = "/Assets/img3.jpg",
                        Location = "Lanzarote,Spanien ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Name = "SwiperSlider4",
                        Caption = "history".ToUpper(),
                        Background = "#b45205",
                        Description = "Awesome Eiffel Tower",
                        ImageSource = "/Assets/img4.jpg",
                        Location = "Paris,France ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Name = "SwiperSlider5",
                        Caption = "Mayans".ToUpper(),
                        Background = "#087ac4",
                        Description = "One of the safest states in Mexico",
                        ImageSource = "/Assets/img5.jpg",
                        Location = "The Yucatan,Mexico ",
                    });
                }



                return carouselSliderList;
            }
            set
            {
                this.OnPropertyChanged(ref carouselSliderList, value);
            }
        }


    }
}
