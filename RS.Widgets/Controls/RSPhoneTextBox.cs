using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace RS.Widgets.Controls
{
    [TemplatePart(Name =nameof(PART_CountryCodeListBox),Type =typeof(ListBox))]
    [TemplatePart(Name = nameof(PART_PopCountryCode), Type = typeof(Popup))]
    public class RSPhoneTextBox : ContentControl
    {
        private ListBox PART_CountryCodeListBox;
        private Popup PART_PopCountryCode;
        static RSPhoneTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSPhoneTextBox), new FrameworkPropertyMetadata(typeof(RSPhoneTextBox)));
        }

        public RSPhoneTextBox()
        {
            CountryCodeModelList = new ObservableCollection<CountryCodeModel>
            {
                new CountryCodeModel()
                {
                    GroupName = "常用地区",
                    Country = "中国大陆",
                    CountryCode = 86
                },

                new CountryCodeModel()
                {
                    GroupName = "常用地区",
                    Country = "中国台湾",
                    CountryCode = 886
                },

                new CountryCodeModel()
                {
                    GroupName = "A",
                    Country = "中国澳门",
                    CountryCode = 853
                },

                new CountryCodeModel()
                {
                    GroupName = "A",
                    Country = "澳大利亚",
                    CountryCode = 61
                },

                new CountryCodeModel()
                {
                    GroupName = "A",
                    Country = "奥地利",
                    CountryCode = 43
                },

                new CountryCodeModel()
                {
                    GroupName = "A",
                    Country = "阿联酋",
                    CountryCode = 971
                }
            };
            //剩下的待处理...
        }


        /// <summary>
        /// 国家代码数据列表
        /// </summary>
        public ObservableCollection<CountryCodeModel> CountryCodeModelList
        {
            get { return (ObservableCollection<CountryCodeModel>)GetValue(CountryCodeModelListProperty); }
            set { SetValue(CountryCodeModelListProperty, value); }
        }
        public static readonly DependencyProperty CountryCodeModelListProperty =
            DependencyProperty.Register("CountryCodeModelList", typeof(ObservableCollection<CountryCodeModel>), typeof(RSPhoneTextBox), new FrameworkPropertyMetadata(null));




        /// <summary>
        /// 绑定的电话
        /// </summary>
        public string Phone
        {
            get { return (string)GetValue(PhoneProperty); }
            set { SetValue(PhoneProperty, value); }
        }
        public static readonly DependencyProperty PhoneProperty =
            DependencyProperty.Register("Phone", typeof(string), typeof(RSPhoneTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPhonePropertyChanged));

        private static void OnPhonePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var phoneTextBox = d as RSPhoneTextBox;
        }

        /// <summary>
        /// 记录国家代码
        /// </summary>
        public int? CountryCode
        {
            get { return (int?)GetValue(CountryCodeProperty); }
            set { SetValue(CountryCodeProperty, value); }
        }

        public static readonly DependencyProperty CountryCodeProperty =
            DependencyProperty.Register("CountryCode", typeof(int?), typeof(RSPhoneTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCountryCodePropertyChanged));

        private static void OnCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var phoneTextBox = d as RSPhoneTextBox;
        }



        public bool IsCloseWindowWhenCountryCodeSelected
        {
            get { return (bool)GetValue(IsCloseWindowWhenCountryCodeSelectedProperty); }
            set { SetValue(IsCloseWindowWhenCountryCodeSelectedProperty, value); }
        }
      
        public static readonly DependencyProperty IsCloseWindowWhenCountryCodeSelectedProperty =
            DependencyProperty.Register("IsCloseWindowWhenCountryCodeSelected", typeof(bool), typeof(RSPhoneTextBox), new PropertyMetadata(true));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

          this.PART_CountryCodeListBox = GetTemplateChild(nameof(PART_CountryCodeListBox)) as ListBox;
            this.PART_PopCountryCode = GetTemplateChild(nameof(PART_PopCountryCode)) as Popup;

            if (this.PART_CountryCodeListBox is not null)
            {
                this.PART_CountryCodeListBox.SelectionChanged -= PART_CountryCodeListBox_SelectionChanged;
                this.PART_CountryCodeListBox.SelectionChanged += PART_CountryCodeListBox_SelectionChanged;
            }
        }

        private void PART_CountryCodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsCloseWindowWhenCountryCodeSelected)
            {
                this.PART_PopCountryCode.IsOpen = false;
            }
        }
    }
}
