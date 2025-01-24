using System;
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

namespace RS.Widgets.Controls
{
    public class RSSerialPort : ContentControl
    {
        private SerialPort serialPort;
        private RSUserControl PART_RSUserControl;
        private DataGrid? PART_DataGrid { get; set; }

        static RSSerialPort()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSerialPort), new FrameworkPropertyMetadata(typeof(RSSerialPort)));
        }

        public RSSerialPort()
        {
            // 初始化命令
            AddDataCommand = new RelayCommand(AddData);
            DeleteCommand = new RelayCommand(DeleteDeviceDataModel);
            BatchImportCommand = new RelayCommand(BatchImport);
            TemplateDownloadCommand = new RelayCommand(TemplateDownload);
            this.Loaded += RSSerialPort_Loaded;
        }

        private void RSSerialPort_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region Command事件
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

        // 批量导入命令依赖属性
        public static readonly DependencyProperty BatchImportCommandProperty =
            DependencyProperty.Register(nameof(BatchImportCommand), typeof(ICommand), typeof(RSSerialPort), new PropertyMetadata(null));

        public ICommand BatchImportCommand
        {
            get { return (ICommand)GetValue(BatchImportCommandProperty); }
            set { SetValue(BatchImportCommandProperty, value); }
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
        private static List<FunctionCodeEnum> functionCodeList;
        /// <summary>
        /// 功能码
        /// </summary>
        public static List<FunctionCodeEnum> FunctionCodeList
        {
            get
            {
                if (functionCodeList == null)
                {
                    functionCodeList = Enum.GetValues<FunctionCodeEnum>().Where(t => t <= FunctionCodeEnum.ReadInputRegisters_0x04).ToList();
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
                deviceDataModel.Address = null;

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
            var duplicateData = dataList.FindDuplicates().ToList();
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

        private async void DeleteDeviceDataModel(object parameter)
        {
            //这里防老年痴呆，得问一问是否删除
            string msg = parameter == "0" ? "你确定要删除该行数据吗" : "你确定要删除所有数据吗?";
            var result = await this.PART_RSUserControl.MessageBox.ShowAsync(msg, null, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            await this.PART_RSUserControl.InvokeLoadingActionAsync(async () =>
            {
                if (parameter == "0")
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

        private void BatchImport(object parameter)
        {

        }

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
