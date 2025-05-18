using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Models
{
    public partial class LoadingConfig : NotifyBase
    {
        //示例
        //new LoadingConfig()
        //{
        //    LoadingType = LoadingType.RotatingAnimation,
        //        IconWidth = 50,
        //        IconHeight = 50,
        //        LoadingColor = new SolidColorBrush(Colors.Red),
        //        LoadingBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88000000")),
        //        IconData = Geometry.Parse("M1016.057791 124.92321l-22.64677 221.938351a28.081995 28.081995 0 0 1-30.799608 25.364383l-221.938351-22.646771a28.081995 28.081995 0 0 1-14.946869-49.822895l76.546085-62.505086a394.053807 394.053807 0 1 0-45.293541 588.816034 58.881603 58.881603 0 1 1 70.657924 94.210565 511.817014 511.817014 0 1 1 65.675634-757.760942l76.99902-62.505087a28.081995 28.081995 0 0 1 45.746476 24.911448z")
        //    }
        public LoadingConfig()
        {
            this.LoadingType = LoadingType.ProgressBar;
            this.IconWidth = 25D;
            this.IconHeight = 25D;
            this.LoadingColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1296db"));
            this.LoadingBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#35000000"));
            this.IsIndeterminate = true;
            this.Minimum = 0;
            this.Maximum = 100;
            this.Value = 0;
            this.GradientStopColor = (Color)ColorConverter.ConvertFromString("#1296db");
        }

        public static LoadingConfig Default
        {
            get
            {
                return new LoadingConfig();
            }
        }

        #region 公用属性设置

        [ObservableProperty]
        private LoadingType loadingType;

        /// <summary>
        /// 加载图标颜色
        /// </summary>
        [ObservableProperty]
        private SolidColorBrush loadingColor;

        /// <summary>
        /// 加载背景色
        /// </summary>
        [ObservableProperty]
        private SolidColorBrush loadingBackground;
        #endregion

        [ObservableProperty]
        private Color gradientStopColor;


        #region 这是默认Progessbar的配置 这里仅仅对Progressbar设置有效
        /// <summary>
        /// 是否自动加载
        /// </summary>
        [ObservableProperty]
        private bool isIndeterminate;

        [ObservableProperty]
        private double maximum;

        [ObservableProperty]
        private double minimum;



        private double _value;
        /// <summary>
        /// 进度条的值
        /// </summary>
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (this.SetProperty(ref _value, value))
                {
                    UpdateLoadingText();
                }
            }
        }

        public string LoadingTextStringFormat { get; set; } = "{0}%";


        private string loadingText;
        /// <summary>
        /// 加载文字
        /// </summary>
        public string LoadingText
        {
            get { return loadingText; }
            set
            {
                this.SetProperty(ref loadingText, value);
            }
        }


        /// <summary>
        /// 是否显示LoadingText
        /// </summary>
        private bool isShowLoadingText;

        public bool IsShowLoadingText
        {
            get { return isShowLoadingText; }
            set
            {
                if (this.SetProperty(ref isShowLoadingText, value))
                {
                    UpdateLoadingText();
                }
            }
        }

        private void UpdateLoadingText()
        {
            if (this.IsShowLoadingText)
            {
                if (string.IsNullOrEmpty(this.LoadingText) && string.IsNullOrEmpty(this.LoadingTextStringFormat))
                {
                    switch (this.LoadingType)
                    {
                        case LoadingType.ProgressBar:
                            this.LoadingText = $"{this.Value}%";
                            break;
                        case LoadingType.RotatingAnimation:
                            this.LoadingText = "正在加载中,请稍后...";
                            break;
                        case LoadingType.BorderSurroundingAnimation:
                            break;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.LoadingTextStringFormat))
                {
                    this.LoadingText = string.Format(this.LoadingTextStringFormat, this.Value);
                }
            }
            else
            {
                this.LoadingText = null;
            }
        }
        #endregion


        #region 这里旋转动画设置

        private Geometry iconData;
        /// <summary>
        /// 加载图标 这个仅在LoadingType是RotatingAnimation情况下有效
        /// </summary>
        public Geometry IconData
        {
            get
            {
                if (iconData == null)
                {
                    iconData = (Geometry)Application.Current.FindResource("RSApp.Icons.DefaultLoadingIcon");
                }
                return iconData;
            }
            set
            {
                this.SetProperty(ref iconData, value);
            }
        }

        /// <summary>
        /// 图标宽度
        /// </summary>
        [ObservableProperty]
        private double iconWidth;

        /// <summary>
        /// 图标高度
        /// </summary>
        [ObservableProperty]
        private double iconHeight;

        #endregion



    }
}
