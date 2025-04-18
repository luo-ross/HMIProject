﻿using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RS.HMI.Client.Views.Home
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class HomeView : RSWindow
    {
        public HomeViewModel ViewModel { get; set; }

        public HomeView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as HomeViewModel;
            this.SizeChanged += HomeView_SizeChanged;
        }

        private void HomeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = this.PART_ContentHost.ActualWidth;
            var height = this.PART_ContentHost.ActualHeight;
            this.PART_ContentHost.Clip = this.GetBorderClipRect(new CornerRadius(10), width, height);
        }
        
        private unsafe void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BtnSearch_OnBtnSearchCallBack(string obj)
        {
            //await this.MessageBox.ShowAsync($@"搜索事件触发, 查询条件{this.ViewModel.Test}");
        }
    
    
    }
}
