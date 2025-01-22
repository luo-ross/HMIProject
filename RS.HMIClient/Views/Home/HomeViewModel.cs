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
        /// <summary>
        /// 串口测试数据
        /// </summary>
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



        private ObservableCollection<DataModel> dataModelList;

        /// <summary>
        /// DataGrid测试数据
        /// </summary>
        public ObservableCollection<DataModel> DataModelList
        {
            get
            {
                if (dataModelList == null)
                {
                    dataModelList = new ObservableCollection<DataModel>();
                    Random random = new Random();
                    for (int i = 0; i < 5; i++)
                    {
                        dataModelList.Add(new DataModel()
                        {
                            DataId = i,
                            Address = (ushort)i,
                            DataDescription = @$"测试数据{i}",
                            DataValue = i * random.NextDouble() * 100,
                            DataGroup = 0,
                            DataType = DataType.String,
                            FunctionCode = 0x03,
                            StationNumber = 0,
                            ReadWritePermission = ReadWriteEnum.ReadWrite,
                            CharacterLength = 0,
                        });
                    }
                }
                return dataModelList;
            }
            set
            {
                this.OnPropertyChanged(ref dataModelList, value);
            }
        }



        private ObservableCollection<ComboBoxItemModel<byte>> functionCodeList;
        /// <summary>
        /// 功能码
        /// </summary>
        public ObservableCollection<ComboBoxItemModel<byte>> FunctionCodeList
        {
            get
            {
                if (functionCodeList == null)
                {
                    functionCodeList = new ObservableCollection<ComboBoxItemModel<byte>>();
                    functionCodeList.Add(new ComboBoxItemModel<byte>()
                    {
                        Key = 0x01,
                        KeyDes = "0x01 读取线圈状态",
                    });
                    functionCodeList.Add(new ComboBoxItemModel<byte>()
                    {
                        Key = 0x02,
                        KeyDes = "0x02 读取离散输入状态",
                    });
                    functionCodeList.Add(new ComboBoxItemModel<byte>()
                    {
                        Key = 0x03,
                        KeyDes = "0x03 读取保持寄存器",
                    });
                    functionCodeList.Add(new ComboBoxItemModel<byte>()
                    {
                        Key = 0x04,
                        KeyDes = "0x04 读取输入寄存器",
                    });
                }
                return functionCodeList;
            }
            set
            {
                this.OnPropertyChanged(ref functionCodeList, value);
            }
        }


        private ObservableCollection<DataType> dataTypeList;
        /// <summary>
        /// 数据类型
        /// </summary>
        public ObservableCollection<DataType> DataTypeList
        {
            get
            {
                if (dataTypeList == null)
                {
                    var dataList = Enum.GetValues<DataType>().ToList();
                    
                    dataTypeList = new ObservableCollection<DataType>(dataList);
                }
                return dataTypeList;
            }
            set
            {
                this.OnPropertyChanged(ref dataTypeList, value);
            }
        }


        private ObservableCollection<ReadWriteEnum> readWritePermissionList;
        /// <summary>
        /// 读取权限
        /// </summary>
        public ObservableCollection<ReadWriteEnum> ReadWritePermissionList
        {
            get
            {
                if (readWritePermissionList == null)
                {
                    var dataList = Enum.GetValues<ReadWriteEnum>().ToList();
                    readWritePermissionList = new ObservableCollection<ReadWriteEnum>(dataList);
                }
                return readWritePermissionList;
            }
            set
            {
                this.OnPropertyChanged(ref readWritePermissionList, value);
            }
        }


        private ObservableCollection<ByteOrderEnum> byteOrderList;
        /// <summary>
        /// 字节序
        /// </summary>
        public ObservableCollection<ByteOrderEnum> ByteOrderList
        {
            get
            {
                if (byteOrderList == null)
                {
                    var dataList = Enum.GetValues<ByteOrderEnum>().ToList();
                    byteOrderList = new ObservableCollection<ByteOrderEnum>(dataList);
                }
                return byteOrderList;
            }
            set
            {
                this.OnPropertyChanged(ref byteOrderList, value);
            }
        }
    }
}
