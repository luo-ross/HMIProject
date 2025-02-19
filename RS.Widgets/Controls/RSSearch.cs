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
        private TextBox PART_TxtSearch;
        static RSSearch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSearch), new FrameworkPropertyMetadata(typeof(RSSearch)));
        }


        [Description("是否字符输入发生变化触发搜索")]
        public bool IsTextChangedSearch
        {
            get { return (bool)GetValue(IsTextChangedSearchProperty); }
            set { SetValue(IsTextChangedSearchProperty, value); }
        }

        public static readonly DependencyProperty IsTextChangedSearchProperty =
            DependencyProperty.Register("IsTextChangedSearch", typeof(bool), typeof(RSSearch), new PropertyMetadata(false));

      

        [Description("搜索内容")]
        public string SearchContent
        {
            get { return (string)GetValue(SearchContentProperty); }
            set { SetValue(SearchContentProperty, value); }
        }

        public static readonly DependencyProperty SearchContentProperty =
            DependencyProperty.Register("SearchContent", typeof(string), typeof(RSSearch), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 搜索事件触发回调
        /// </summary>
        public event Action<string> OnBtnSearchCallBack;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnSearch = this.GetTemplateChild(nameof(this.PART_BtnSearch)) as Button;
            this.PART_TxtSearch = this.GetTemplateChild(nameof(this.PART_TxtSearch)) as TextBox;

            if (this.PART_BtnSearch != null)
            {
                this.PART_BtnSearch.Click -= PART_BtnSearch_Click;
                this.PART_BtnSearch.Click += PART_BtnSearch_Click;
            }

            if (this.PART_TxtSearch != null)
            {
                this.PART_TxtSearch.TextChanged -= this.PART_TxtSearch_TextChanged;
                this.PART_TxtSearch.TextChanged += this.PART_TxtSearch_TextChanged;
            }
        }

        private void PART_TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsTextChangedSearch)
            {
                this.OnBtnSearchCallBack?.Invoke(this.SearchContent);
            }
        }

        private void PART_BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.OnBtnSearchCallBack?.Invoke(this.SearchContent);
        }
    }
}
