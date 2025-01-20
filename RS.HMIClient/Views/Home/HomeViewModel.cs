using RS.HMIClient.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.BorderWindowDemo.Views.Home
{
    public class HomeViewModel : NotifyBase
    {
        public HomeViewModel()
        {
            this.BtnClickCommand = new RelayCommand(BtnClick, CanBtnClick);      
        }

        private bool CanBtnClick(object arg)
        {
            return true;
        }

        private void BtnClick(object obj)
        {
            MessageBox.Show("这是MVVM命令事件");
        }


        private string test;
        /// <summary>
        /// 搜索内容
        /// </summary>
        public string Test
        {
            get { return test; }
            set
            {
                this.OnPropertyChanged(ref test, value);
            }
        }


        private bool isFullScreen;

        public bool IsFullScreen
        {
            get { return isFullScreen; }
            set
            {
                isFullScreen = value;
                this.OnPropertyChanged();
            }
        }

        private ICommand btnClickCommand;

        public ICommand BtnClickCommand
        {
            get { return btnClickCommand; }
            set
            {
                btnClickCommand = value;
                this.OnPropertyChanged();
            }
        }

    }
}
