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
    public class LoginViewModel : NotifyBase
    {
        public LoginViewModel()
        {
            InitCarouselSlider();
        }


        private void InitCarouselSlider()
        {
            for (int i = 0; i < this.CarouselSliderList.Count; i++)
            {
                var item = this.CarouselSliderList[i];
                MySlider mySlider = new MySlider();
                mySlider.Name = item.Name;
                mySlider.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFrom(item.Background);
                mySlider.Caption = item.Caption;
                mySlider.Description = item.Description;
                mySlider.ImageSource = item.ImageSource;
                mySlider.Location = item.Location;
                mySlider.Focusable = false;
                mySlider.Tag = item;

                RSCarouselSliderList.Add(mySlider);
            }

            this.OnPropertyChanged(nameof(RSCarouselSliderList));
        }

        private ObservableCollection<FrameworkElement> rsCarouselSliderList;
        /// <summary>
        /// 测试数据集
        /// </summary>
        public ObservableCollection<FrameworkElement> RSCarouselSliderList
        {
            get
            {
                if (rsCarouselSliderList == null)
                {
                    rsCarouselSliderList = new ObservableCollection<FrameworkElement>();
                }
                return rsCarouselSliderList;
            }
            set
            {
                this.OnPropertyChanged(ref rsCarouselSliderList, value);
            }
        }



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



        private double offsetX;
        public double OffsetX
        {
            get
            {

                return offsetX;
            }
            set
            {
                this.OnPropertyChanged(ref offsetX, value);
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




        private List<CarouselSlider> carouselSliderList;
        /// <summary>
        /// 轮播数据集
        /// </summary>
        public List<CarouselSlider> CarouselSliderList
        {
            get
            {
                if (carouselSliderList == null)
                {
                    carouselSliderList = new List<CarouselSlider>();

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 0,
                        Name = "SwiperSlider1",
                        Caption = "native".ToUpper(),
                        Background = "#1b7402",
                        Description = "The most popular yachting destination",
                        ImageSource = "/Assets/img1.jpg",
                        Location = "Whitsunday Islands,Australia ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 1,
                        Name = "SwiperSlider2",
                        Caption = "domestic".ToUpper(),
                        Background = "#62667f",
                        Description = "Enjoy the exotic of sunny Hawaii",
                        ImageSource = "/Assets/img2.jpg",
                        Location = "Maui,Hawaii ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 2,
                        Name = "SwiperSlider3",
                        Caption = "subtropical".ToUpper(),
                        Background = "#087ac4",
                        Description = "The Island of Eternal Spring",
                        ImageSource = "/Assets/img3.jpg",
                        Location = "Lanzarote,Spanien ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 3,
                        Name = "SwiperSlider4",
                        Caption = "history".ToUpper(),
                        Background = "#b45205",
                        Description = "Awesome Eiffel Tower",
                        ImageSource = "/Assets/img4.jpg",
                        Location = "Paris,France ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 4,
                        Name = "SwiperSlider5",
                        Caption = "Mayans".ToUpper(),
                        Background = "#087ac4",
                        Description = "One of the safest states in Mexico",
                        ImageSource = "/Assets/img5.jpg",
                        Location = "The Yucatan,Mexico ",
                    });


                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 5,
                        Name = "SwiperSlider1",
                        Caption = "native".ToUpper(),
                        Background = "#1b7402",
                        Description = "The most popular yachting destination",
                        ImageSource = "/Assets/img1.jpg",
                        Location = "Whitsunday Islands,Australia ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 6,
                        Name = "SwiperSlider2",
                        Caption = "domestic".ToUpper(),
                        Background = "#62667f",
                        Description = "Enjoy the exotic of sunny Hawaii",
                        ImageSource = "/Assets/img2.jpg",
                        Location = "Maui,Hawaii ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 7,
                        Name = "SwiperSlider3",
                        Caption = "subtropical".ToUpper(),
                        Background = "#087ac4",
                        Description = "The Island of Eternal Spring",
                        ImageSource = "/Assets/img3.jpg",
                        Location = "Lanzarote,Spanien ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 8,
                        Name = "SwiperSlider4",
                        Caption = "history".ToUpper(),
                        Background = "#b45205",
                        Description = "Awesome Eiffel Tower",
                        ImageSource = "/Assets/img4.jpg",
                        Location = "Paris,France ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 9,
                        Name = "SwiperSlider5",
                        Caption = "Mayans".ToUpper(),
                        Background = "#087ac4",
                        Description = "One of the safest states in Mexico",
                        ImageSource = "/Assets/img5.jpg",
                        Location = "The Yucatan,Mexico ",
                    });


                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 10,
                        Name = "SwiperSlider1",
                        Caption = "native".ToUpper(),
                        Background = "#1b7402",
                        Description = "The most popular yachting destination",
                        ImageSource = "/Assets/img1.jpg",
                        Location = "Whitsunday Islands,Australia ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 11,
                        Name = "SwiperSlider2",
                        Caption = "domestic".ToUpper(),
                        Background = "#62667f",
                        Description = "Enjoy the exotic of sunny Hawaii",
                        ImageSource = "/Assets/img2.jpg",
                        Location = "Maui,Hawaii ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 12,
                        Name = "SwiperSlider3",
                        Caption = "subtropical".ToUpper(),
                        Background = "#087ac4",
                        Description = "The Island of Eternal Spring",
                        ImageSource = "/Assets/img3.jpg",
                        Location = "Lanzarote,Spanien ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 13,
                        Name = "SwiperSlider4",
                        Caption = "history".ToUpper(),
                        Background = "#b45205",
                        Description = "Awesome Eiffel Tower",
                        ImageSource = "/Assets/img4.jpg",
                        Location = "Paris,France ",
                    });

                    carouselSliderList.Add(new CarouselSlider()
                    {
                        Index = 14,
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
                carouselSliderList = value;
            }
        }


       


    }
}
