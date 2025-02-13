﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Timers;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using RS.Widgets.Models;
using RS.Widgets.Common.Enums;
using System.Windows.Input;
using RS.Commons.Extensions;
using RS.Commons.Compares;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using RS.Widgets.Common.Excels;
using NPOI.SS.Formula.Functions;
using MathNet.Numerics.Distributions;
using System.Data;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.Reflection.PortableExecutable;

namespace RS.Widgets.Controls
{
    public class RSSerialPort : ContentControl
    {
        public static readonly string CellValueEditErrorKey = "8E5424EEEDDB4BCE8AA634C684811672";

        private SerialPort serialPort;
        private RSUserControl PART_RSUserControl;
        private DataGrid? PART_DataGrid { get; set; }

        static RSSerialPort()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSerialPort), new FrameworkPropertyMetadata(typeof(RSSerialPort)));
        }

        public RSSerialPort()
        {
            // 添加数据命令
            AddDataCommand = new RelayCommand(AddData);

            //删除配置命令
            DeleteCommand = new RelayCommand<string>(DeleteDeviceDataModel);

            //导入配置命令
            ImportConfigCommand = new RelayCommand(ImportConfig);

            //导入配置命令
            ExportConfigCommand = new RelayCommand(ExportConfig);

            //模版下载命令
            TemplateDownloadCommand = new RelayCommand(TemplateDownload);

            //DataId更改事件
            CellValueEditChangedCommand = new RelayCommand<string>(CellValueEditChanged);

            this.Loaded += RSSerialPort_Loaded;
            this.DeviceDataModelList = new ObservableCollection<DeviceDataModel>();
        }



        /// <summary>
        /// 单元格数据编辑更改事件
        /// </summary>
        /// <param name="property">编辑属性名称</param>
        private void CellValueEditChanged(string property)
        {
            List<DeviceDataModel> deviceDataModelList = new List<DeviceDataModel>();
            this.Dispatcher.Invoke(() =>
            {
                deviceDataModelList = this.DeviceDataModelList.ToList();
            });
            switch (property)
            {
                //数据标签
                case nameof(DeviceDataModel.DataId):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var dataId = item.DataId;
                            //判断DataId是否重复  
                            if (deviceDataModelList.Count(t => t.DataId == dataId) > 1)
                            {
                                ICollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                                validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult("数据编号重复"));
                                item.AddErrors(nameof(DeviceDataModel.DataId), validationResults, CellValueEditErrorKey);
                            }
                            else
                            {
                                item.RemoveErrors(nameof(DeviceDataModel.DataId), CellValueEditErrorKey);
                            }
                        }
                    }
                    break;

                //通讯站号
                case nameof(DeviceDataModel.StationNumber):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var stationNumber = item.StationNumber;
                        }
                    }
                    break;

                //功能码
                case nameof(DeviceDataModel.FunctionCode):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var functionCode = item.FunctionCode;
                        }
                    }
                    break;

                //读取地址
                case nameof(DeviceDataModel.Address):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var address = item.Address;
                        }
                    }
                    break;

                //数据类型
                case nameof(DeviceDataModel.DataType):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var dataType = item.DataType;
                        }
                    }
                    break;

                //字符长度
                case nameof(DeviceDataModel.CharacterLength):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var characterLength = item.CharacterLength;
                            //判断字符串长度 
                            if (characterLength < 0)
                            {
                                ICollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                                validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult("长度不能小于0"));
                                item.AddErrors(nameof(DeviceDataModel.CharacterLength), validationResults, CellValueEditErrorKey);
                            }
                            else
                            {
                                item.RemoveErrors(nameof(DeviceDataModel.CharacterLength), CellValueEditErrorKey);
                            }
                        }
                    }
                    break;

                //读写权限
                case nameof(DeviceDataModel.ReadWritePermission):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var readWritePermission = item.ReadWritePermission;
                        }
                    }
                    break;

                //字节序
                case nameof(DeviceDataModel.ByteOrder):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var byteOrder = item.ByteOrder;
                        }
                    }
                    break;

                //数据分组
                case nameof(DeviceDataModel.DataGroup):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var dataGroup = item.DataGroup;
                        }
                    }
                    break;

                //数据描述
                case nameof(DeviceDataModel.DataDescription):
                    {
                        foreach (var item in deviceDataModelList)
                        {
                            var dataDescription = item.DataDescription;

                            //判断DataDescription是否重复
                            if (deviceDataModelList.Count(t => !string.IsNullOrWhiteSpace(t.DataDescription) && t.DataDescription?.Trim() == dataDescription?.Trim()) > 1)
                            {
                                ICollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                                validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult("数据描述重复"));
                                item.AddErrors(nameof(DeviceDataModel.DataDescription), validationResults, CellValueEditErrorKey);
                            }
                            else
                            {
                                item.RemoveErrors(nameof(DeviceDataModel.DataDescription), CellValueEditErrorKey);
                            }
                        }
                    }
                    break;
            }
        }

        private void RSSerialPort_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region Command事件
        //DataId更改事件
        public static readonly DependencyProperty CellValueEditChangedCommandProperty =
            DependencyProperty.Register(nameof(CellValueEditChangedCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand CellValueEditChangedCommand
        {
            get { return (ICommand)GetValue(CellValueEditChangedCommandProperty); }
            set { SetValue(CellValueEditChangedCommandProperty, value); }
        }


        // 新增数据命令依赖属性
        public static readonly DependencyProperty AddDataCommandProperty =
            DependencyProperty.Register(nameof(AddDataCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand AddDataCommand
        {
            get { return (ICommand)GetValue(AddDataCommandProperty); }
            set { SetValue(AddDataCommandProperty, value); }
        }

        // 删除选中命令依赖属性
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        // 导入配置命令依赖属性
        public static readonly DependencyProperty ImportConfigCommandProperty =
            DependencyProperty.Register(nameof(ImportConfigCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand ImportConfigCommand
        {
            get { return (ICommand)GetValue(ImportConfigCommandProperty); }
            set { SetValue(ImportConfigCommandProperty, value); }
        }


        // 导出配置命令依赖属性
        public static readonly DependencyProperty ExportConfigCommandProperty =
            DependencyProperty.Register(nameof(ExportConfigCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand ExportConfigCommand
        {
            get { return (ICommand)GetValue(ExportConfigCommandProperty); }
            set { SetValue(ExportConfigCommandProperty, value); }
        }

        // 模版下载命令依赖属性
        public static readonly DependencyProperty TemplateDownloadCommandProperty =
            DependencyProperty.Register(nameof(TemplateDownloadCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand TemplateDownloadCommand
        {
            get { return (ICommand)GetValue(TemplateDownloadCommandProperty); }
            set { SetValue(TemplateDownloadCommandProperty, value); }
        }
        #endregion


        [Description("波特率")]
        [DefaultValue(9600)]
        public int BaudRate
        {
            get { return (int)GetValue(BaudRateProperty); }
            set { SetValue(BaudRateProperty, value); }
        }

        public static readonly DependencyProperty BaudRateProperty =
            DependencyProperty.Register("BaudRate", typeof(int), typeof(RSSerialPort), new PropertyMetadata(9600));

        [Description("串口名称")]
        [DefaultValue(null)]
        public string PortName
        {
            get { return (string)GetValue(PortNameProperty); }
            set { SetValue(PortNameProperty, value); }
        }

        public static readonly DependencyProperty PortNameProperty =
            DependencyProperty.Register("PortName", typeof(string), typeof(RSSerialPort), new PropertyMetadata(null));

        [Description("握手协议")]
        [DefaultValue(Handshake.None)]
        public Handshake Handshake
        {
            get { return (Handshake)GetValue(HandshakeProperty); }
            set { SetValue(HandshakeProperty, value); }
        }

        public static readonly DependencyProperty HandshakeProperty =
            DependencyProperty.Register("Handshake", typeof(Handshake), typeof(RSSerialPort), new PropertyMetadata(Handshake.None));


        [Description("停止位")]
        [DefaultValue(StopBits.One)]
        public StopBits StopBits
        {
            get { return (StopBits)GetValue(StopBitsProperty); }
            set { SetValue(StopBitsProperty, value); }
        }

        public static readonly DependencyProperty StopBitsProperty =
            DependencyProperty.Register("StopBits", typeof(StopBits), typeof(RSSerialPort), new PropertyMetadata(StopBits.One));


        [Description("数据位")]
        [DefaultValue(8)]
        public int DataBits
        {
            get { return (int)GetValue(DataBitsProperty); }
            set { SetValue(DataBitsProperty, value); }
        }

        public static readonly DependencyProperty DataBitsProperty =
            DependencyProperty.Register("DataBits", typeof(int), typeof(RSSerialPort), new PropertyMetadata(8));


        [Description("奇偶校验")]
        [DefaultValue(Parity.None)]
        public Parity Parity
        {
            get { return (Parity)GetValue(ParityProperty); }
            set { SetValue(ParityProperty, value); }
        }

        public static readonly DependencyProperty ParityProperty =
            DependencyProperty.Register("Parity", typeof(Parity), typeof(RSSerialPort), new PropertyMetadata(Parity.None));


        [Description("是否连接")]
        [DefaultValue(false)]
        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }
        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(false));


        [Description("是否自动重连")]
        [DefaultValue(true)]
        public bool AutoReconnect
        {
            get { return (bool)GetValue(AutoReconnectProperty); }
            set { SetValue(AutoReconnectProperty, value); }
        }

        public static readonly DependencyProperty AutoReconnectProperty =
            DependencyProperty.Register("AutoReconnect", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(false));



        [Description("重连间隔(毫秒)")]
        [DefaultValue(1000)]
        public int ReconnectInterval
        {
            get { return (int)GetValue(ReconnectIntervalProperty); }
            set { SetValue(ReconnectIntervalProperty, value); }
        }

        public static readonly DependencyProperty ReconnectIntervalProperty =
            DependencyProperty.Register("ReconnectInterval", typeof(int), typeof(RSSerialPort), new PropertyMetadata(1000));


        [Description("最大重连尝试次数，0表示无限重试")]
        [DefaultValue(0)]
        public int MaxReconnectAttempts
        {
            get { return (int)GetValue(MaxReconnectAttemptsProperty); }
            set { SetValue(MaxReconnectAttemptsProperty, value); }
        }

        public static readonly DependencyProperty MaxReconnectAttemptsProperty =
            DependencyProperty.Register("MaxReconnectAttempts", typeof(int), typeof(RSSerialPort), new PropertyMetadata(0));


        [Description("设备数据")]
        [DefaultValue(null)]
        public ObservableCollection<DeviceDataModel> DeviceDataModelList
        {
            get { return (ObservableCollection<DeviceDataModel>)GetValue(DeviceDataModelListProperty); }
            set { SetValue(DeviceDataModelListProperty, value); }
        }
        public static readonly DependencyProperty DeviceDataModelListProperty =
            DependencyProperty.Register("DeviceDataModelList", typeof(ObservableCollection<DeviceDataModel>), typeof(RSSerialPort), new PropertyMetadata(null));



        [Description("通讯状态描述")]
        [DefaultValue(null)]
        public string CommuStatusDes
        {
            get { return (string)GetValue(CommuStatusDesProperty); }
            set { SetValue(CommuStatusDesProperty, value); }
        }

        public static readonly DependencyProperty CommuStatusDesProperty =
            DependencyProperty.Register("CommuStatusDes", typeof(string), typeof(RSSerialPort), new PropertyMetadata(null));



        [Description("标题")]
        [DefaultValue("串口通讯")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RSSerialPort), new PropertyMetadata("串口通讯"));




        [Description("地址是否从0开始")]
        [DefaultValue(true)]
        public bool IsAddressStartZero
        {
            get { return (bool)GetValue(IsAddressStartZeroProperty); }
            set { SetValue(IsAddressStartZeroProperty, value); }
        }

        public static readonly DependencyProperty IsAddressStartZeroProperty =
            DependencyProperty.Register("IsAddressStartZero", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(true));


        [Description("是否进行CRC16校验")]
        [DefaultValue(true)]
        public bool IsCrc16Checked
        {
            get { return (bool)GetValue(IsCrc16CheckedProperty); }
            set { SetValue(IsCrc16CheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCrc16CheckedProperty =
            DependencyProperty.Register("IsCrc16Checked", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(true));



        [Description("是否字符串颠倒")]
        [DefaultValue(false)]
        public bool IsStringInverse
        {
            get { return (bool)GetValue(IsStringInverseProperty); }
            set { SetValue(IsStringInverseProperty, value); }
        }

        public static readonly DependencyProperty IsStringInverseProperty =
            DependencyProperty.Register("IsStringInverse", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(false));



        [Description("是否自动连接")]
        [DefaultValue(false)]
        public bool IsAutoConnect
        {
            get { return (bool)GetValue(IsAutoConnectProperty); }
            set { SetValue(IsAutoConnectProperty, value); }
        }

        public static readonly DependencyProperty IsAutoConnectProperty =
            DependencyProperty.Register("IsAutoConnect", typeof(bool), typeof(RSSerialPort), new PropertyMetadata(false));


        [Description("配置选中项")]
        public DeviceDataModel DeviceDataModelSelected
        {
            get { return (DeviceDataModel)GetValue(DeviceDataModelSelectedProperty); }
            set { SetValue(DeviceDataModelSelectedProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataModelSelectedProperty =
            DependencyProperty.Register("DeviceDataModelSelected", typeof(DeviceDataModel), typeof(RSSerialPort), new PropertyMetadata(null));





        #region 通用数据
        private static List<ComboBoxItemModel<FunctionCodeEnum>> functionCodeList;
        /// <summary>
        /// 功能码
        /// </summary>
        public static List<ComboBoxItemModel<FunctionCodeEnum>> FunctionCodeList
        {
            get
            {
                if (functionCodeList == null)
                {
                    functionCodeList = new List<ComboBoxItemModel<FunctionCodeEnum>>();
                    functionCodeList.Add(new ComboBoxItemModel<FunctionCodeEnum>()
                    {
                        Key = FunctionCodeEnum.ReadCoils_0x01,
                        KeyDes = "01(0x01)- 读取线圈状态"
                    });
                    functionCodeList.Add(new ComboBoxItemModel<FunctionCodeEnum>()
                    {
                        Key = FunctionCodeEnum.ReadDiscreteInputs_0x02,
                        KeyDes = "02(0x02)-读取离散输入 "
                    });
                    functionCodeList.Add(new ComboBoxItemModel<FunctionCodeEnum>()
                    {
                        Key = FunctionCodeEnum.ReadHoldingRegisters_0x03,
                        KeyDes = "03(0x03)-读取保持寄存器 "
                    });
                    functionCodeList.Add(new ComboBoxItemModel<FunctionCodeEnum>()
                    {
                        Key = FunctionCodeEnum.ReadInputRegisters_0x04,
                        KeyDes = "04(0x04)-读取输入寄存器 "
                    });
                }
                return functionCodeList;
            }
        }


        private static List<DataTypeEnum> dataTypeList;
        /// <summary>
        /// 数据类型
        /// </summary>
        public static List<DataTypeEnum> DataTypeList
        {
            get
            {
                if (dataTypeList == null)
                {
                    dataTypeList = Enum.GetValues<DataTypeEnum>().ToList();
                }
                return dataTypeList;
            }
        }


        private static List<ReadWriteEnum> readWritePermissionList;
        /// <summary>
        /// 读取权限
        /// </summary>
        public static List<ReadWriteEnum> ReadWritePermissionList
        {
            get
            {
                if (readWritePermissionList == null)
                {
                    readWritePermissionList = Enum.GetValues<ReadWriteEnum>().ToList();
                }
                return readWritePermissionList;
            }
        }


        private static List<ByteOrderEnum> byteOrderList;
        /// <summary>
        /// 字节序
        /// </summary>
        public static List<ByteOrderEnum> ByteOrderList
        {
            get
            {
                if (byteOrderList == null)
                {
                    byteOrderList = Enum.GetValues<ByteOrderEnum>().ToList();
                }
                return byteOrderList;
            }
        }


        private static List<int> dataBitsList;
        /// <summary>
        ///  数据位
        /// </summary>
        public static List<int> DataBitsList
        {
            get
            {
                if (dataBitsList == null)
                {
                    dataBitsList = new List<int>()
                    {
                      5,6,7,8
                    };
                }
                return dataBitsList;
            }
        }

        private static List<int> baudRateList;
        /// <summary>
        /// 波特率
        /// </summary>
        public static List<int> BaudRateList
        {
            get
            {
                if (baudRateList == null)
                {
                    baudRateList = new List<int>()
                    {
                      1200,2400,4800,9600,19200,38400,57600,115200
                    };
                }
                return baudRateList;
            }
        }




        private static List<Parity> parityList;
        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public static List<Parity> ParityList
        {
            get
            {
                if (parityList == null)
                {
                    parityList = Enum.GetValues<Parity>().ToList();
                }
                return parityList;
            }
        }


        private static List<StopBits> stopBitsList;
        /// <summary>
        /// 停止位
        /// </summary>
        public static List<StopBits> StopBitsList
        {
            get
            {
                if (stopBitsList == null)
                {
                    stopBitsList = Enum.GetValues<StopBits>().ToList();
                }
                return stopBitsList;
            }
        }




        #endregion


        #region 事件

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<string> DataReceived;

        /// <summary>
        /// 错误发生事件
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// 重连尝试事件
        /// </summary>
        public event EventHandler<int> ReconnectAttempted;

        /// <summary>
        /// 重连成功事件
        /// </summary>
        public event EventHandler ReconnectSucceeded;

        /// <summary>
        /// 重连失败事件（达到最大重试次数）
        /// </summary>
        public event EventHandler ReconnectFailed;

        #endregion


        #region Command实现

        private DeviceDataModel DeviceDataModelAdd;
        private async void AddData(object parameter)
        {
            if (this.DeviceDataModelList == null)
            {
                this.DeviceDataModelList = new ObservableCollection<DeviceDataModel>();
            }
            var seviceDataModelSelected = this.DeviceDataModelSelected;
            var deviceDataModelList = this.DeviceDataModelList.ToList();
            var operateResult = await this.PART_RSUserControl.InvokeLoadingActionAsync(async () =>
            {
                //首先进行数据验证
                var deviceDataModelValidResult = DeviceDataModelValid(deviceDataModelList);
                if (!deviceDataModelValidResult.IsSuccess)
                {
                    return deviceDataModelValidResult;
                }

                //如果用户没有选中行
                if (seviceDataModelSelected == null)
                {
                    //我们就获取列表最后一个数据
                    seviceDataModelSelected = deviceDataModelList.LastOrDefault();
                }

                DeviceDataModel deviceDataModel = null;
                if (seviceDataModelSelected != null)
                {
                    DeviceDataModelAdd = seviceDataModelSelected.Clone();
                }

                if (deviceDataModel == null)
                {
                    //验证通过继续下一步
                    deviceDataModel = new DeviceDataModel();
                }

                deviceDataModel.DataDescription = null;

                if (deviceDataModelList.Count > 0)
                {
                    deviceDataModel.DataId = deviceDataModelList.Max(t => t.DataId) + 1;
                }

                //主动触发一次校验 告诉用户哪些地方需要修改
                deviceDataModel.ValidObject();
                this.Dispatcher.Invoke(() =>
                {
                    this.DeviceDataModelList.Add(deviceDataModel);
                    this.DeviceDataModelSelected = deviceDataModel;
                });

                return OperateResult.CreateResult();
            });

            if (!operateResult.IsSuccess)
            {
                await this.PART_RSUserControl.MessageBox.ShowAsync(operateResult.Message, null, MessageBoxButton.OK, icon: MessageBoxImage.Warning);
            }
        }

        private OperateResult DeviceDataModelValid(List<DeviceDataModel> dataList)
        {
            if (dataList.Count == 0)
            {
                return OperateResult.CreateResult();
            }

            //添加之前验证我们的数据是否都符合规定
            foreach (var item in dataList)
            {
                //每一个数据输入验证通过之后
                if (!item.ValidObject())
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        //如果验证不通过设置选中
                        this.DeviceDataModelSelected = item;
                        this.ScrollDeviceDataModelIntoView(this.DeviceDataModelSelected);
                    });
                    return WarningOperateResult.CreateResult("数据验证不通过，不能继续新增数据！");
                }
            }
            //还需要验证数据配置是否有重复
            var duplicateData = GetDuplicateData(dataList);
            if (duplicateData.Count > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.DeviceDataModelSelected = duplicateData.FirstOrDefault();
                    this.ScrollDeviceDataModelIntoView(this.DeviceDataModelSelected);
                });
                return WarningOperateResult.CreateResult("数据配置重复！");
            }
            return OperateResult.CreateResult();
        }

        private void ScrollDeviceDataModelIntoView(DeviceDataModel deviceDataModel)
        {
            this.PART_DataGrid?.ScrollIntoView(deviceDataModel);
        }

        /// <summary>
        /// 删除数据配置
        /// </summary>
        /// <param name="parameter">0 删除单行 1删除全部</param>
        private async void DeleteDeviceDataModel(string parameter)
        {
            //这里防老年痴呆，得问一问是否删除
            string msg = parameter.Equals("0") ? "你确定要删除该行数据吗" : "你确定要删除所有数据吗?";
            var result = await this.PART_RSUserControl.MessageBox.ShowAsync(msg, null, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            await this.PART_RSUserControl.InvokeLoadingActionAsync(async () =>
            {
                if (parameter.Equals("0"))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.DeviceDataModelList.Remove(this.DeviceDataModelSelected);
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.DeviceDataModelList.Clear();
                    });
                }

                return OperateResult.CreateResult();
            });


        }

        private List<DeviceDataModel> GetDuplicateData(List<DeviceDataModel> dataList, List<string> exceptPropertyList = null)
        {
            var validPropertyList = new List<string>()
            {
                nameof(DeviceDataModel.DataId),
                nameof(DeviceDataModel.StationNumber),
                nameof(DeviceDataModel.FunctionCode),
                nameof(DeviceDataModel.Address),
                nameof(DeviceDataModel.DataType),
                nameof(DeviceDataModel.CharacterLength),
                nameof(DeviceDataModel.ReadWritePermission),
                nameof(DeviceDataModel.ByteOrder),
                nameof(DeviceDataModel.DataGroup),
                nameof(DeviceDataModel.DataDescription),
            };

            if (exceptPropertyList != null)
            {
                validPropertyList = validPropertyList.Except(exceptPropertyList).ToList();
            }
            return dataList.FindDuplicates(validPropertyList).ToList();
        }

        /// <summary>
        /// 导入参数配置
        /// </summary>
        /// <param name="parameter"></param>
        private async void ImportConfig(object parameter)
        {
            //这里我们需要打开一个文件选择框
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置Excel文件的过滤器
            openFileDialog.Filter = "Excel 文件 (*.xls;*.xlsx)|*.xls;*.xlsx";
            // 显示对话框并检查用户是否点击了确定
            if (openFileDialog.ShowDialog() == true)
            {
                // 获取选定的文件路径
                string filePath = openFileDialog.FileName;

                //获取数据副本
                var deviceDataModelList = this.DeviceDataModelList.ToList();
                var operateResult = await this.PART_RSUserControl.InvokeLoadingActionAsync(async () =>
                  {
                      //获取Excel工作簿
                      IWorkbook workbook = ExcelHelper.GetWorkbook(filePath);

                      // 读取数据配置表
                      ISheet sheet = workbook.GetSheet("DataConfig");

                      //读取串口通讯配置
                      this.GetSerialPortConfig(sheet);

                      //读取数据配置
                      var dataList = this.GetDeviceDataModelConfig(workbook, sheet);

                      deviceDataModelList = deviceDataModelList.Concat(dataList).ToList();

                      //还需要验证数据配置是否有重复
                      var duplicateData = GetDuplicateData(deviceDataModelList);

                      //获取数据的差集
                      dataList = dataList.Except(duplicateData).ToList();

                      foreach (var item in dataList)
                      {
                          this.Dispatcher.Invoke(() =>
                          {
                              //自动获取DataId
                              if (item.DataId == -1 && deviceDataModelList.Count > 0)
                              {
                                  item.DataId = deviceDataModelList.Max(t => t.DataId) + 1;
                              }
                              this.DeviceDataModelList.Add(item);
                          });
                      }

                      //触发数据验证
                      CellValueEditChanged(nameof(DeviceDataModel.DataId));
                      CellValueEditChanged(nameof(DeviceDataModel.StationNumber));
                      CellValueEditChanged(nameof(DeviceDataModel.FunctionCode));
                      CellValueEditChanged(nameof(DeviceDataModel.Address));
                      CellValueEditChanged(nameof(DeviceDataModel.DataType));
                      CellValueEditChanged(nameof(DeviceDataModel.CharacterLength));
                      CellValueEditChanged(nameof(DeviceDataModel.ReadWritePermission));
                      CellValueEditChanged(nameof(DeviceDataModel.ByteOrder));
                      CellValueEditChanged(nameof(DeviceDataModel.DataGroup));
                      CellValueEditChanged(nameof(DeviceDataModel.DataDescription));
                      return OperateResult.CreateResult();
                  });

                if (!operateResult.IsSuccess)
                {
                    await this.PART_RSUserControl.MessageBox.ShowAsync(operateResult.Message, null, MessageBoxButton.OK, icon: MessageBoxImage.Warning);
                }
            }
        }


        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="parameter"></param>
        private async void ExportConfig(object parameter)
        {
            //这里我们需要打开一个文件选择框
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // 设置Excel文件的过滤器
            saveFileDialog.Filter = "Excel 文件 (*.xlsx;*.xls)|*.xlsx;*.xls";
            saveFileDialog.Title = "导出通讯配置";
            // 显示对话框并检查用户是否点击了确定
            if (saveFileDialog.ShowDialog() == true)
            {
                // 获取选定的文件路径
                string filePath = saveFileDialog.FileName;

                //获取数据副本
                var deviceDataModelList = this.DeviceDataModelList.ToList();
                var operateResult = await this.PART_RSUserControl.InvokeLoadingActionAsync(async () =>
                {
                    //获取Excel工作簿
                    IWorkbook workbook = ExcelHelper.CreateWorkbook(filePath);

                    // 创建一个工作表
                    ISheet sheet = workbook.CreateSheet("DataConfig");

                    var style1 = workbook.CreateCellStyle();
                    style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    style1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                    style1.FillPattern = FillPattern.SolidForeground;

                    var style2 = workbook.CreateCellStyle();
                    style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    style2.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;




                    //导出第一行配置
                    IRow currentRow = sheet.CreateRow(0);

                    //Com口
                    var cell = currentRow.CreateCell(0);
                    cell.CellStyle = style1;
                    cell.SetCellValue("Com口");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(1);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.PortName);
                    });

                    //波特率
                    cell = currentRow.CreateCell(2);
                    cell.CellStyle = style1;
                    cell.SetCellValue("波特率");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(3);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.BaudRate);
                    });

                    //数据位
                    cell = currentRow.CreateCell(4);
                    cell.CellStyle = style1;
                    cell.SetCellValue("数据位");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(5);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.DataBits);
                    });

                    //停止位
                    cell = currentRow.CreateCell(6);
                    cell.CellStyle = style1;
                    cell.SetCellValue("停止位");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(7);
                        cell.CellStyle = style2;
                        cell.SetCellValue((int)this.StopBits);
                    });

                    //奇偶校验位
                    cell = currentRow.CreateCell(8);
                    cell.CellStyle = style1;
                    cell.SetCellValue("奇偶校验位");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(9);
                        cell.CellStyle = style2;
                        cell.SetCellValue((int)this.Parity);
                    });


                    //第2行配置
                    currentRow = sheet.CreateRow(1);

                    //地址是否从0开始
                    cell = currentRow.CreateCell(0);
                    cell.CellStyle = style1;
                    cell.SetCellValue("是否从0开始");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(1);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.IsAddressStartZero);
                    });

                    //是否进行Crc16校验
                    cell = currentRow.CreateCell(2);
                    cell.CellStyle = style1;
                    cell.SetCellValue("是否CRC校验");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(3);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.IsCrc16Checked);
                    });

                    //是否字符串颠倒
                    cell = currentRow.CreateCell(4);
                    cell.CellStyle = style1;
                    cell.SetCellValue("是否字符串颠倒");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(5);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.IsStringInverse);
                    });

                    //是否自动连接
                    cell = currentRow.CreateCell(6);
                    cell.CellStyle = style1;
                    cell.SetCellValue("是否自动连接");
                    this.Dispatcher.Invoke(() =>
                    {
                        cell = currentRow.CreateCell(7);
                        cell.CellStyle = style2;
                        cell.SetCellValue(this.IsAutoConnect);
                    });

                    //第3行配置
                    currentRow = sheet.CreateRow(2);

                    //第4行配置
                    currentRow = sheet.CreateRow(3);
                    cell = currentRow.CreateCell(0);
                    cell.CellStyle = style1;
                    cell.SetCellValue("数据编号");

                    cell = currentRow.CreateCell(1);
                    cell.CellStyle = style1;
                    cell.SetCellValue("通讯站号");

                    cell = currentRow.CreateCell(2);
                    cell.CellStyle = style1;
                    cell.SetCellValue("功能码");

                    cell = currentRow.CreateCell(3);
                    cell.CellStyle = style1;
                    cell.SetCellValue("地址");

                    cell = currentRow.CreateCell(4);
                    cell.CellStyle = style1;
                    cell.SetCellValue("数据类型");

                    cell = currentRow.CreateCell(5);
                    cell.CellStyle = style1;
                    cell.SetCellValue("字符长度");

                    cell = currentRow.CreateCell(6);
                    cell.CellStyle = style1;
                    cell.SetCellValue("读写权限");

                    cell = currentRow.CreateCell(7);
                    cell.CellStyle = style1;
                    cell.SetCellValue("字节顺序");

                    cell = currentRow.CreateCell(8);
                    cell.CellStyle = style1;
                    cell.SetCellValue("数据分组");

                    cell = currentRow.CreateCell(9);
                    cell.CellStyle = style1;
                    cell.SetCellValue("数据描述");
                    //导出数据配置
                    int totalRow = deviceDataModelList.Count();

                    for (int i = 0; i < totalRow; i++)
                    {
                        //获取数据配置
                        var dataConfig = deviceDataModelList[i];
                        //数据配置从第4行开始
                        currentRow = sheet.CreateRow(i + 4);
                        cell = currentRow.CreateCell(0);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.DataId);

                        cell = currentRow.CreateCell(1);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.StationNumber);

                        cell = currentRow.CreateCell(2);
                        cell.CellStyle = style2;
                        cell.SetCellValue((int)dataConfig.FunctionCode);

                        cell = currentRow.CreateCell(3);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.Address);

                        cell = currentRow.CreateCell(4);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.DataType.ToString());

                        cell = currentRow.CreateCell(5);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.CharacterLength.HasValue ? dataConfig.CharacterLength.ToString() : null);

                        cell = currentRow.CreateCell(6);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.ReadWritePermission.ToString());

                        cell = currentRow.CreateCell(7);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.ByteOrder.ToString());

                        cell = currentRow.CreateCell(8);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.DataGroup.ToString());

                        cell = currentRow.CreateCell(9);
                        cell.CellStyle = style2;
                        cell.SetCellValue(dataConfig.DataDescription);
                    }

                    int totalCol = 10;
                    //动态设置列的宽度
                    for (int col = 0; col < totalCol; col++)
                    {
                        //获取最大列
                        int maxByteLength = 0;
                        for (int row = 0; row < totalRow + 4; row++)
                        {
                            var cellValue = sheet.GetRow(row).GetCell(col)?.ToString();
                            var currentByteLength = GetByteLength(cellValue);
                            if (currentByteLength > maxByteLength)
                            {
                                maxByteLength = currentByteLength;
                            }
                        }
                        sheet.SetColumnWidth(col, 256 * (maxByteLength + 1));
                    }

                    // 设置表头固定（冻结第4行）
                    sheet.CreateFreezePane(0, 4);

                    // 设置表头筛选 行列是从0开始
                    int firstRow = 3; // 表头所在行
                    int lastRow = totalRow + 4 - 1;  // 数据最后一行
                    int firstCol = 0; // 第一列
                    int lastCol = totalCol - 1; // 最后一列
                    CellRangeAddress cellRangeAddress = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
                    sheet.SetAutoFilter(cellRangeAddress);

                    // 保存文件
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs);
                    }
                    return OperateResult.CreateResult();
                });

                if (!operateResult.IsSuccess)
                {
                    await this.PART_RSUserControl.MessageBox.ShowAsync(operateResult.Message, null, MessageBoxButton.OK, icon: MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// 计算字符串的字节长度，汉字按 2 个字节，英语大写字母按 2 个字节，英语小写字母和其他单字节字符按 1 个字节
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <returns>字符串的字节长度</returns>
        public static int GetByteLength(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }

            int length = 0;
            foreach (char c in input)
            {
                // 判断字符是否为高代理项（处理代理对，如表情符号等）
                if (char.IsHighSurrogate(c))
                {
                    // 处理代理对，跳过下一个低代理字符
                    length += 4;
                    continue;
                }
                if (IsChineseCharacter(c) || char.IsUpper(c))
                {
                    length += 2;
                }
                else
                {
                    length += 1;
                }
            }
            return length;
        }


        /// <summary>
        /// 判断字符是否为中文字符
        /// </summary>
        /// <param name="c">要判断的字符</param>
        /// <returns>如果是中文字符返回 true，否则返回 false</returns>
        private static bool IsChineseCharacter(char c)
        {
            // 中文字符的 Unicode 范围
            return c >= '\u4e00' && c <= '\u9fff';
        }



        /// <summary>
        /// 获取串口通讯配置
        /// </summary>
        /// <param name="sheet"></param>
        private void GetSerialPortConfig(ISheet sheet)
        {
            //获取第一行配置
            IRow currentRow = sheet.GetRow(0);
            //获取Com口
            string portName = currentRow.GetCell(1)?.ToString();
            this.Dispatcher.Invoke(() =>
            {
                this.PortName = portName;
            });

            //获取波特率
            if (int.TryParse(currentRow.GetCell(3)?.ToString(), out int baudRate))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.BaudRate = baudRate;
                });
            }

            //获取数据位
            if (int.TryParse(currentRow.GetCell(5)?.ToString(), out int dataBits))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.DataBits = dataBits;
                });
            }

            //获取停止位
            if (Enum.TryParse(currentRow.GetCell(7)?.ToString(), true, out StopBits stopBits))
            {
                if (stopBits >= StopBits.None && stopBits <= StopBits.OnePointFive)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.StopBits = stopBits;
                    });
                }
            }

            //获取奇偶校验位
            if (Enum.TryParse(currentRow.GetCell(9)?.ToString(), true, out Parity parity))
            {
                if (parity >= Parity.None && parity <= Parity.Space)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Parity = parity;
                    });
                }
            }

            //获取第2行配置
            currentRow = sheet.GetRow(1);

            //获取数据地址是否从0开始
            if (bool.TryParse(currentRow.GetCell(1)?.ToString(), out bool isAddressStartZero))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsAddressStartZero = isAddressStartZero;
                });
            }

            //获取是否进行Crc16校验
            if (bool.TryParse(currentRow.GetCell(3)?.ToString(), out bool isCrc16Checked))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsCrc16Checked = isCrc16Checked;
                });
            }

            //获取字符串读取是否反转
            if (bool.TryParse(currentRow.GetCell(5)?.ToString(), out bool isStringInverse))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsStringInverse = isStringInverse;
                });
            }

            //获取通讯是否自动连接
            if (bool.TryParse(currentRow.GetCell(7)?.ToString(), out bool isAutoConnect))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsAutoConnect = isAutoConnect;
                });
            }

        }

        /// <summary>
        /// 获取数据配置
        /// </summary>
        /// <param name="filePath">配置文件绝对路径</param>
        /// <returns></returns>
        private List<DeviceDataModel> GetDeviceDataModelConfig(IWorkbook workbook, ISheet sheet)
        {
            List<DeviceDataModel> deviceDataModelList = new List<DeviceDataModel>();
            // 遍历行和单元格并读取数据
            for (int row = 4; row <= sheet.LastRowNum; row++)
            {
                DeviceDataModel deviceDataModel = new DeviceDataModel();
                IRow currentRow = sheet.GetRow(row);
                if (currentRow != null)
                {
                    //读取数据标签
                    if (int.TryParse(currentRow.GetCell(0)?.ToString(), out int dataId))
                    {
                        deviceDataModel.DataId = dataId;
                    }


                    //读取通讯站号
                    if (byte.TryParse(currentRow.GetCell(1)?.ToString(), out byte stationNumber))
                    {
                        deviceDataModel.StationNumber = stationNumber;
                    }

                    //读取功能码 
                    if (Enum.TryParse(currentRow.GetCell(2)?.ToString(), true, out FunctionCodeEnum functionCode))
                    {
                        deviceDataModel.FunctionCode = functionCode;
                    }

                    //读取地址
                    if (int.TryParse(currentRow.GetCell(3)?.ToString(), out int address))
                    {
                        deviceDataModel.Address = address;
                    }
                    //读取数据类型
                    if (Enum.TryParse(currentRow.GetCell(4)?.ToString(), true, out DataTypeEnum dataType))
                    {
                        deviceDataModel.DataType = dataType;
                    }

                    //只有数据类型为字符串时才读取字符长度
                    if (deviceDataModel.DataType == DataTypeEnum.String)
                    {
                        //读取字符长度
                        if (int.TryParse(currentRow.GetCell(5)?.ToString(), out int characterLength))
                        {
                            deviceDataModel.CharacterLength = characterLength;
                        }
                    }

                    //读取读取权限
                    if (Enum.TryParse(currentRow.GetCell(6)?.ToString(), true, out ReadWriteEnum readWritePermission))
                    {
                        if (readWritePermission == ReadWriteEnum.Read || readWritePermission == ReadWriteEnum.ReadWrite)
                        {
                            deviceDataModel.ReadWritePermission = readWritePermission;
                        }
                    }

                    //读取字节顺序
                    if (Enum.TryParse(currentRow.GetCell(7)?.ToString(), true, out ByteOrderEnum byteOrder))
                    {
                        deviceDataModel.ByteOrder = byteOrder;
                    }

                    //读取数据分组
                    if (byte.TryParse(currentRow.GetCell(8)?.ToString(), out byte dataGroup))
                    {
                        deviceDataModel.DataGroup = dataGroup;
                    }

                    //读取数据描述
                    deviceDataModel.DataDescription = currentRow.GetCell(9)?.ToString();
                    deviceDataModelList.Add(deviceDataModel);
                }
            }

            return deviceDataModelList;
        }

        /// <summary>
        /// 模板下载
        /// </summary>
        /// <param name="parameter"></param>
        private void TemplateDownload(object parameter)
        {

        }
        #endregion


        /// <summary>
        /// 连接串口
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool Connect()
        {
            try
            {
                if (IsConnected)
                {
                    return true;
                }

                serialPort = new SerialPort
                {
                    PortName = this.PortName,
                    BaudRate = this.BaudRate,
                    Parity = this.Parity,
                    DataBits = this.DataBits,
                    StopBits = this.StopBits
                };

                // 设置数据接收事件处理
                serialPort.DataReceived += SerialPort_DataReceived;
                // 设置错误事件处理
                serialPort.ErrorReceived += SerialPort_ErrorReceived;
                // 设置引脚变化事件处理
                serialPort.PinChanged += SerialPort_PinChanged;
                serialPort.Open();
                IsConnected = true;
                // 连接成功，重置重连计数
                currentReconnectAttempts = 0;
                StopReconnectTimer();
                return true;
            }
            catch (Exception ex)
            {
                // 如果启用了自动重连，开始重连
                if (AutoReconnect)
                {
                    StartReconnectTimer();
                }
                return false;
            }
        }

        /// <summary>
        /// 断开串口连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                StopReconnectTimer();

                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    serialPort.PinChanged -= SerialPort_PinChanged;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                }
                IsConnected = false;
            }
            catch (Exception ex)
            {
                //ErrorOccurred?.Invoke(this, ex);
            }
        }


        /// <summary>
        /// 串口错误接收处理
        /// </summary>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            var error = new Exception($"串口错误: {e.EventType}");
            ErrorOccurred?.Invoke(this, error);

            // 如果发生错误且启用了自动重连，断开当前连接并开始重连
            if (AutoReconnect)
            {
                Disconnect();
                StartReconnectTimer();
            }
        }


        /// <summary>
        /// 串口引脚变化处理
        /// </summary>
        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            // 如果是断开连接相关的引脚变化
            if (e.EventType == SerialPinChange.Break ||
                e.EventType == SerialPinChange.Ring)
            {
                IsConnected = false;

                // 如果启用了自动重连，开始重连
                if (AutoReconnect)
                {

                }
            }
        }


        /// <summary>
        /// 串口数据接收处理
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private System.Timers.Timer reconnectTimer;

        /// <summary>
        /// 初始化重连定时器
        /// </summary>
        private void InitializeReconnectTimer()
        {
            reconnectTimer = new System.Timers.Timer(ReconnectInterval);
            reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            reconnectTimer.AutoReset = true;
        }

        private int currentReconnectAttempts;
        /// <summary>
        /// 重连定时器触发事件处理
        /// </summary>
        private void ReconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 如果已经连接，停止重连
            if (IsConnected)
            {
                StopReconnectTimer();
                return;
            }

            // 检查是否超过最大重试次数
            if (MaxReconnectAttempts > 0 && currentReconnectAttempts >= MaxReconnectAttempts)
            {
                StopReconnectTimer();
                ReconnectFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            currentReconnectAttempts++;
            ReconnectAttempted?.Invoke(this, currentReconnectAttempts);

            // 尝试重新连接
            if (Connect())
            {
                ReconnectSucceeded?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 启动重连定时器
        /// </summary>
        private void StartReconnectTimer()
        {
            if (reconnectTimer == null)
            {
                InitializeReconnectTimer();
            }
            currentReconnectAttempts = 0;
            reconnectTimer.Start();
        }


        /// <summary>
        /// 停止重连定时器
        /// </summary>
        private void StopReconnectTimer()
        {
            reconnectTimer?.Stop();
            currentReconnectAttempts = 0;
        }


        /// <summary>
        /// 获取可用串口列表
        /// </summary>
        /// <returns>串口名称列表</returns>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_RSUserControl = this.GetTemplateChild(nameof(this.PART_RSUserControl)) as RSUserControl;
            this.PART_DataGrid = this.GetTemplateChild(nameof(this.PART_DataGrid)) as DataGrid;
        }
    }
}
