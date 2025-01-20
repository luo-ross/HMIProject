using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSSearch : ContentControl
    {
        private Button PART_BtnSearch;
        static RSSearch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSearch), new FrameworkPropertyMetadata(typeof(RSSearch)));
        }

        /// <summary>
        /// 搜索内容
        /// </summary>
        public string SearchContent
        {
            get { return (string)GetValue(SearchContentProperty); }
            set { SetValue(SearchContentProperty, value); }
        }

        public static readonly DependencyProperty SearchContentProperty =
            DependencyProperty.Register("SearchContent", typeof(string), typeof(RSSearch), new PropertyMetadata(string.Empty));


        public event Action<string> OnBtnSearchCallBack;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnSearch = this.GetTemplateChild(nameof(
            this.PART_BtnSearch)) as Button;

            if (this.PART_BtnSearch != null)
            {
                this.PART_BtnSearch.Click -= PART_BtnSearch_Click;
                this.PART_BtnSearch.Click += PART_BtnSearch_Click;
            }
        }

        private void PART_BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.OnBtnSearchCallBack?.Invoke(this.SearchContent);
        }
    }
}
