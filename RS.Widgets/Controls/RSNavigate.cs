using MathNet.Numerics;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RS.Widgets.Controls
{
    public class RSNavigate : Frame
    {
        private RSWindow ParentWin;
        private RSSearch PART_Search;
        private Border PART_NavHost;
    
        static RSNavigate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSNavigate), new FrameworkPropertyMetadata(typeof(RSNavigate)));
        }

        public RSNavigate()
        {
            this.Loaded += RSNavigate_Loaded;
        }

        private void RSNavigate_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ParentWin == null)
            {
                this.ParentWin = this.TryFindParent<RSWindow>();
                if (this.ParentWin != null)
                {
                    this.ParentWin.SizeChanged -= ParentWin_SizeChanged;
                    this.ParentWin.SizeChanged += ParentWin_SizeChanged;
                }
            }

            this.UpdateNavType();
        }

        private void ParentWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateNavType();
        }

        private void UpdateNavType()
        {
            if (this.ParentWin == null)
            {
                return;
            }

            var width = this.ParentWin.Width;

            //尺寸小于1000的时候显示迷你导航
            //尺寸小于650 隐藏迷你导航
            //尺寸在1000以内时 导航展开就是浮动导航
            //尺寸在1000以上  导航展开就是宽度自适应

            //展开的时候根据屏幕尺寸判断是浮动还是宽度自适应
            //缩小的时候根据屏幕判断是显示迷你版导航还是隐藏导航
            if (width <= 650)
            {
                this.ScreenSize = ScreenSize.Small;
                this.NavType = NavType.Floating;
            }
            else if (width <= 1000)
            {
                this.ScreenSize = ScreenSize.Medium;
                this.NavType = NavType.Floating;
            }
            else
            {
                this.ScreenSize = ScreenSize.Large;
                this.NavType = NavType.AdaptiveWidth;
            }
        }



        public double NavNormalWidth
        {
            get { return (double)GetValue(NavNormalWidthProperty); }
            set { SetValue(NavNormalWidthProperty, value); }
        }

        public static readonly DependencyProperty NavNormalWidthProperty =
            DependencyProperty.Register("NavNormalWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(280D, OnNavNormalWidthPropertyChanged));

        private static void OnNavNormalWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

        public double NavMinWidth
        {
            get { return (double)GetValue(NavMinWidthProperty); }
            set { SetValue(NavMinWidthProperty, value); }
        }

        public static readonly DependencyProperty NavMinWidthProperty =
            DependencyProperty.Register("NavMinWidth", typeof(double), typeof(RSNavigate), new PropertyMetadata(50D, OnNavMinWidthPropertyChanged));

        private static void OnNavMinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }


        public ScreenSize ScreenSize
        {
            get { return (ScreenSize)GetValue(ScreenSizeProperty); }
            set { SetValue(ScreenSizeProperty, value); }
        }

        public static readonly DependencyProperty ScreenSizeProperty =
            DependencyProperty.Register("ScreenSize", typeof(ScreenSize), typeof(RSNavigate), new PropertyMetadata(ScreenSize.Unknown, OnScreenSizePropertyChanged));

        private static void OnScreenSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

        public NavStatus NavStatus
        {
            get { return (NavStatus)GetValue(NavStatusProperty); }
            set { SetValue(NavStatusProperty, value); }
        }

        public static readonly DependencyProperty NavStatusProperty =
            DependencyProperty.Register("NavStatus", typeof(NavStatus), typeof(RSNavigate), new PropertyMetadata(NavStatus.Unknown, OnNavStatusPropertyChanged));

        private static void OnNavStatusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

        public NavType NavType
        {
            get { return (NavType)GetValue(NavTypeProperty); }
            set { SetValue(NavTypeProperty, value); }
        }

        public static readonly DependencyProperty NavTypeProperty =
            DependencyProperty.Register("NavType", typeof(NavType), typeof(RSNavigate), new PropertyMetadata(NavType.Unknown, OnNavTypePropertyChanged));

        private static void OnNavTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }

        public object BodyCotent
        {
            get { return (object)GetValue(BodyCotentProperty); }
            set { SetValue(BodyCotentProperty, value); }
        }

        public static readonly DependencyProperty BodyCotentProperty =
            DependencyProperty.Register("BodyCotent", typeof(object), typeof(RSNavigate), new PropertyMetadata(null));


        public bool IsNavExpanded
        {
            get { return (bool)GetValue(IsNavExpandedProperty); }
            set { SetValue(IsNavExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsNavExpandedProperty =
            DependencyProperty.Register("IsNavExpanded", typeof(bool), typeof(RSNavigate), new PropertyMetadata(true, OnIsNavExpandedPropertyChanged));

        private static void OnIsNavExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNavigate = (RSNavigate)d;
            rsNavigate.UpateNavVisualState();
        }
    
        private void UpateNavVisualState()
        {
            //手动优先级最高
            if (this.IsNavExpanded)
            {
                this.NavStatus = NavStatus.Expanded;
            }
            else
            {
                this.NavStatus = NavStatus.Collapsed;
            }

            switch (this.ScreenSize)
            {
                case ScreenSize.Unknown:
                    return;
                case ScreenSize.Small:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(0, TimeSpan.FromSeconds(0));
                    }
                    break;
                case ScreenSize.Medium:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(this.NavMinWidth, TimeSpan.FromSeconds(0));
                    }
                    break;
                case ScreenSize.Large:
                    if (this.NavStatus == NavStatus.Expanded)
                    {
                        AnimateNavHostWidth(this.NavNormalWidth, TimeSpan.FromSeconds(0.2));
                    }
                    else
                    {
                        AnimateNavHostWidth(this.NavMinWidth, TimeSpan.FromSeconds(0.2));
                    }
                    break;
            }
        }
     

        private void AnimateNavHostWidth(double toWidth, TimeSpan duration)
        {
            var animation = new DoubleAnimation
            {
                To = toWidth,
                Duration = duration
            };
            this.PART_NavHost?.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_Search = this.GetTemplateChild(nameof(this.PART_Search)) as RSSearch;
            this.PART_NavHost = this.GetTemplateChild(nameof(this.PART_NavHost)) as Border;
            if (this.PART_Search != null)
            {
                this.PART_Search.OnBtnSearchCallBack -= PART_Search_OnBtnSearchCallBack;
                this.PART_Search.OnBtnSearchCallBack += PART_Search_OnBtnSearchCallBack;
            }

        }

        private void PART_Search_OnBtnSearchCallBack(string obj)
        {
            if (!this.IsNavExpanded)
            {
                this.IsNavExpanded = true;
            }
        }
    }
}
