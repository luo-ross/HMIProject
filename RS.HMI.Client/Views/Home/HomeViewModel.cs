using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Home
{
    public class User : NotifyBase
    {
        public int Id { get; set; }
     

        private string name;
        /// <summary>
        /// 中文
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                this.OnPropertyChanged(ref name, value);
            }
        }

        private string name2;
        /// <summary>
        /// 英文
        /// </summary>
        public string Name2
        {
            get { return name2; }
            set
            {
                this.OnPropertyChanged(ref name2, value);
            }
        }
    }


    public class HomeViewModel : NotifyBase
    {

        public HomeViewModel()
        {
            this.BtnClickCommand = new RelayCommand(BtnClick, CanBtnClick);

            this.DataList = new ObservableCollection<User>();
            for (int i = 0; i < 10; i++)
            {
                this.DataList.Add(new User()
                {
                    Id = i + 1,
                    Name = $"Ross{i + 1}",
                    Name2 = $"螺丝{i + 1}",
                });
            }
        }


        private bool isEnglish;

        public bool IsEnglish
        {
            get { return isEnglish; }
            set
            {
                this.OnPropertyChanged(ref isEnglish, value);
            }
        }


        private ObservableCollection<User> dataList;

        public ObservableCollection<User> DataList
        {
            get { return dataList; }
            set
            {
                this.OnPropertyChanged(ref dataList, value);
            }
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
                this.OnPropertyChanged(ref isFullScreen, value);
            }
        }

        private ICommand btnClickCommand;

        public ICommand BtnClickCommand
        {
            get { return btnClickCommand; }
            set
            {
                this.OnPropertyChanged(ref btnClickCommand, value);
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
