using RS.HMIClient.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO.Ports;

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


        private string searchContent;
        /// <summary>
        /// 搜索内容
        /// </summary>
        public string SearchContent
        {
            get { return searchContent; }
            set
            {
                this.OnPropertyChanged(ref searchContent, value);
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





        private ObservableCollection<string> serialPortNameList;

        public ObservableCollection<string> SerialPortNameList
        {
            get
            {
                if (serialPortNameList == null)
                {
                    // 获取所有可用串口
                    var portList = SerialPort.GetPortNames().ToList();
                    serialPortNameList = new ObservableCollection<string>(portList);

                    for (int i = 0; i < 5; i++)
                    {
                        serialPortNameList.Add($@"Com{i + 1}");
                    }
                }
                return serialPortNameList;
            }
            set
            {
                this.OnPropertyChanged(ref serialPortNameList, value);
            }
        }

    }
}
