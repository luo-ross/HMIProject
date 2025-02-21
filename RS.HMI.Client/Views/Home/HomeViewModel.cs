using RS.HMI.Models.Widgets;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Home
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





        //private ObservableCollection<string> serialPortNameList;
        ///// <summary>
        ///// 串口测试数据
        ///// </summary>
        //public ObservableCollection<string> SerialPortNameList
        //{
        //    get
        //    {
        //        if (serialPortNameList == null)
        //        {
        //            // 获取所有可用串口
        //            var portList = SerialPort.GetPortNames().ToList();
        //            serialPortNameList = new ObservableCollection<string>(portList);

        //            for (int i = 0; i < 5; i++)
        //            {
        //                serialPortNameList.Add($@"Com{i + 1}");
        //            }
        //        }
        //        return serialPortNameList;
        //    }
        //    set
        //    {
        //        this.OnPropertyChanged(ref serialPortNameList, value);
        //    }
        //}



       

        //private ObservableCollection< CommuStation> commuStationList;
        ///// <summary>
        ///// 通讯站列表
        ///// </summary>
        //public ObservableCollection<CommuStation> CommuStationList
        //{
        //    get
        //    {
        //        if (commuStationList == null)
        //        {
        //            commuStationList = new ObservableCollection<CommuStation>();
        //        }
        //        return commuStationList;
        //    }
        //    set
        //    {
        //        this.OnPropertyChanged(ref commuStationList, value);
        //    }
        //}


     
    }
}
