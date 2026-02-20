using RS.Commons;
using RS.Commons.Enums;
using RS.OmniComLib.DataConfigs;
using RS.Widgets.Controls;
using RS.Widgets.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.OmniComLib.Controls
{
    public class RSSerialPort1 : UserControl
    {
        private int generatorId = 1;
        private SerialPort SerialPort;
        private RSDialog PART_RSDialog;
        private DataGrid? PART_DataGrid;
        private Button PART_BtnConnect;
        private Button PART_BtnDisConnect;
        private Button PART_BtnSaveConfig;
        private RSWindow ParentWin;

        public CancellationTokenSource ConnectCTS { get; set; }

        /// <summary>
        /// 唯一主键
        /// </summary>
        private string Id { get; set; }
        public RSSerialPort1()
        {
            //// 添加数据命令
            //this.SetValue(AddCommandPropertyKey, new RelayCommand<object>(AddData));

            ////删除配置命令
            //this.SetValue(DeleteCommandPropertyKey, new RelayCommand<string>(DeleteModbusDataConfigModel));

            ////导入配置命令
            //this.SetValue(ImportConfigCommandPropertyKey, new RelayCommand<object>(ImportConfig));

            ////导入配置命令
            //this.SetValue(ExportCommandPropertyKey, new RelayCommand<object>(ExportConfig));

            ////模版下载命令
            //this.SetValue(TemplateDownloadCommandPropertyKey, new RelayCommand<object>(TemplateDownload));

            ////DataId更改事件
            //this.SetValue(CellValueEditChangedCommandPropertyKey, new RelayCommand<string>(CellValueEditChanged));

            //this.Loaded += RSSerialPort_Loaded;
            //this.ModbusDataConfigModelList = new ObservableCollection<ModbusDataConfig>();
        }



        private void RSSerialPort_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWin = this.TryFindParent<RSWindow>();
        }

        #region Command事件

        private static readonly DependencyPropertyKey CellValueEditChangedCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CellValueEditChangedCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));
        public static readonly DependencyProperty CellValueEditChangedCommandProperty = CellValueEditChangedCommandPropertyKey.DependencyProperty;
        public ICommand CellValueEditChangedCommand
        {
            get { return (ICommand)GetValue(CellValueEditChangedCommandProperty); }
        }


        // 新增数据命令依赖属性
        private static readonly DependencyPropertyKey AddCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(AddCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));
        public static readonly DependencyProperty AddCommandProperty = AddCommandPropertyKey.DependencyProperty;
        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
        }

        // 删除选中命令依赖属性
        private static readonly DependencyPropertyKey DeleteCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DeleteCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty = DeleteCommandPropertyKey.DependencyProperty;
        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
        }

        // 导入配置命令依赖属性
        private static readonly DependencyPropertyKey ImportConfigCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImportConfigCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));
        public static readonly DependencyProperty ImportConfigCommandProperty = ImportConfigCommandPropertyKey.DependencyProperty;
        public ICommand ImportConfigCommand
        {
            get { return (ICommand)GetValue(ImportConfigCommandProperty); }
        }


        // 导出配置命令依赖属性
        private static readonly DependencyPropertyKey ExportCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ExportCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));
        public static readonly DependencyProperty ExportCommandProperty = ExportCommandPropertyKey.DependencyProperty;
        public ICommand ExportCommand
        {
            get { return (ICommand)GetValue(ExportCommandProperty); }
        }

        // 模版下载命令依赖属性
        private static readonly DependencyPropertyKey TemplateDownloadCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TemplateDownloadCommand), typeof(ICommand), typeof(RSSerialPort1), new PropertyMetadata(null));
        public static readonly DependencyProperty TemplateDownloadCommandProperty = TemplateDownloadCommandPropertyKey.DependencyProperty;
        public ICommand TemplateDownloadCommand
        {
            get { return (ICommand)GetValue(TemplateDownloadCommandProperty); }
        }
        #endregion

        #region 依赖属性

        [Description("波特率")]
        [DefaultValue(9600)]
        public int BaudRate
        {
            get { return (int)GetValue(BaudRateProperty); }
            set { SetValue(BaudRateProperty, value); }
        }

        public static readonly DependencyProperty BaudRateProperty =
            DependencyProperty.Register("BaudRate", typeof(int), typeof(RSSerialPort1), new PropertyMetadata(9600, OnBaudRatePropertyChanged));

        private static void OnBaudRatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        [Description("串口名称")]
        [DefaultValue(null)]
        public string PortName
        {
            get { return (string)GetValue(PortNameProperty); }
            set { SetValue(PortNameProperty, value); }
        }

        public static readonly DependencyProperty PortNameProperty =
            DependencyProperty.Register("PortName", typeof(string), typeof(RSSerialPort1), new PropertyMetadata(null));

        [Description("握手协议")]
        [DefaultValue(Handshake.None)]
        public Handshake Handshake
        {
            get { return (Handshake)GetValue(HandshakeProperty); }
            set { SetValue(HandshakeProperty, value); }
        }

        public static readonly DependencyProperty HandshakeProperty =
            DependencyProperty.Register("Handshake", typeof(Handshake), typeof(RSSerialPort1), new PropertyMetadata(Handshake.None));


        [Description("停止位")]
        [DefaultValue(StopBits.One)]
        public StopBits StopBits
        {
            get { return (StopBits)GetValue(StopBitsProperty); }
            set { SetValue(StopBitsProperty, value); }
        }

        public static readonly DependencyProperty StopBitsProperty =
            DependencyProperty.Register("StopBits", typeof(StopBits), typeof(RSSerialPort1), new PropertyMetadata(StopBits.One));


        [Description("数据位")]
        [DefaultValue(8)]
        public int DataBits
        {
            get { return (int)GetValue(DataBitsProperty); }
            set { SetValue(DataBitsProperty, value); }
        }

        public static readonly DependencyProperty DataBitsProperty =
            DependencyProperty.Register("DataBits", typeof(int), typeof(RSSerialPort1), new PropertyMetadata(8));


        [Description("奇偶校验")]
        [DefaultValue(Parity.None)]
        public Parity Parity
        {
            get { return (Parity)GetValue(ParityProperty); }
            set { SetValue(ParityProperty, value); }
        }

        public static readonly DependencyProperty ParityProperty =
            DependencyProperty.Register("Parity", typeof(Parity), typeof(RSSerialPort1), new PropertyMetadata(Parity.None));


        [Description("是否连接")]
        [DefaultValue(false)]
        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }
        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(RSSerialPort1), new PropertyMetadata(false));


        [Description("是否自动重连")]
        [DefaultValue(true)]
        public bool AutoReconnect
        {
            get { return (bool)GetValue(AutoReconnectProperty); }
            set { SetValue(AutoReconnectProperty, value); }
        }

        public static readonly DependencyProperty AutoReconnectProperty =
            DependencyProperty.Register("AutoReconnect", typeof(bool), typeof(RSSerialPort1), new PropertyMetadata(false));



        [Description("重连间隔(毫秒)")]
        [DefaultValue(1000)]
        public int ReconnectInterval
        {
            get { return (int)GetValue(ReconnectIntervalProperty); }
            set { SetValue(ReconnectIntervalProperty, value); }
        }

        public static readonly DependencyProperty ReconnectIntervalProperty =
            DependencyProperty.Register("ReconnectInterval", typeof(int), typeof(RSSerialPort1), new PropertyMetadata(1000));


        [Description("最大重连尝试次数，0表示无限重试")]
        [DefaultValue(0)]
        public int MaxReconnectAttempts
        {
            get { return (int)GetValue(MaxReconnectAttemptsProperty); }
            set { SetValue(MaxReconnectAttemptsProperty, value); }
        }

        public static readonly DependencyProperty MaxReconnectAttemptsProperty =
            DependencyProperty.Register("MaxReconnectAttempts", typeof(int), typeof(RSSerialPort1), new PropertyMetadata(0));


        [Description("设备数据")]
        [DefaultValue(null)]
        public ObservableCollection<ModbusDataConfig> ModbusDataConfigModelList
        {
            get { return (ObservableCollection<ModbusDataConfig>)GetValue(ModbusDataConfigModelListProperty); }
            set { SetValue(ModbusDataConfigModelListProperty, value); }
        }
        public static readonly DependencyProperty ModbusDataConfigModelListProperty =
            DependencyProperty.Register("ModbusDataConfigModelList", typeof(ObservableCollection<ModbusDataConfig>), typeof(RSSerialPort1), new PropertyMetadata(null));



        [Description("通讯状态描述")]
        [DefaultValue(null)]
        public string CommuStatusDes
        {
            get { return (string)GetValue(CommuStatusDesProperty); }
            set { SetValue(CommuStatusDesProperty, value); }
        }

        public static readonly DependencyProperty CommuStatusDesProperty =
            DependencyProperty.Register("CommuStatusDes", typeof(string), typeof(RSSerialPort1), new PropertyMetadata(null));



        [Description("标题")]
        [DefaultValue("串口通讯")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RSSerialPort1), new PropertyMetadata("串口通讯"));



        [Description("是否自动连接")]
        [DefaultValue(false)]
        public bool IsAutoConnect
        {
            get { return (bool)GetValue(IsAutoConnectProperty); }
            set { SetValue(IsAutoConnectProperty, value); }
        }

        public static readonly DependencyProperty IsAutoConnectProperty =
            DependencyProperty.Register("IsAutoConnect", typeof(bool), typeof(RSSerialPort1), new PropertyMetadata(false));


        [Description("配置选中项")]
        public ModbusDataConfig ModbusDataConfigModelSelected
        {
            get { return (ModbusDataConfig)GetValue(ModbusDataConfigModelSelectedProperty); }
            set { SetValue(ModbusDataConfigModelSelectedProperty, value); }
        }

        public static readonly DependencyProperty ModbusDataConfigModelSelectedProperty =
            DependencyProperty.Register("ModbusDataConfigModelSelected", typeof(ModbusDataConfig), typeof(RSSerialPort1), new PropertyMetadata(null));



        [Description("是否连接成功")]
        public bool? IsConnectSuccess
        {
            get { return (bool?)GetValue(IsConnectSuccessProperty); }
            set { SetValue(IsConnectSuccessProperty, value); }
        }

        public static readonly DependencyProperty IsConnectSuccessProperty =
            DependencyProperty.Register("IsConnectSuccess", typeof(bool?), typeof(RSSerialPort1), new PropertyMetadata(null));


        [Description("通讯时间")]
        public DateTime CommunicationTime
        {
            get { return (DateTime)GetValue(CommunicationTimeProperty); }
            set { SetValue(CommunicationTimeProperty, value); }
        }
        public static readonly DependencyProperty CommunicationTimeProperty =
            DependencyProperty.Register("CommunicationTime", typeof(DateTime), typeof(RSSerialPort1), new PropertyMetadata(null));

        #endregion

        #region 通用数据
        // 添加静态串口列表属性
        private static List<string> serialPortNameList;
        public static List<string> SerialPortNameList
        {
            get
            {
                if (serialPortNameList == null)
                {
                    serialPortNameList = SerialPort.GetPortNames().ToList();
                    serialPortNameList.Sort();
                }
                return serialPortNameList;
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



        /// <summary>
        /// 连接串口
        /// </summary>
        /// <returns>是否连接成功</returns>
        public OperateResult Connect()
        {
            try
            {
                if (IsConnected)
                {
                    return OperateResult.CreateSuccessResult();
                }
                this.SerialPort = new SerialPort
                {
                    PortName = this.PortName,
                    BaudRate = this.BaudRate,
                    Parity = this.Parity,
                    DataBits = this.DataBits,
                    StopBits = this.StopBits
                };
                // 设置数据接收事件处理
                this.SerialPort.DataReceived += SerialPort_DataReceived;
                // 设置错误事件处理
                this.SerialPort.ErrorReceived += SerialPort_ErrorReceived;
                // 设置引脚变化事件处理
                this.SerialPort.PinChanged += SerialPort_PinChanged;
                this.SerialPort.Open();
                IsConnected = true;
                return OperateResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return OperateResult.CreateFailResult(ex.Message);
            }
        }

        /// <summary>
        /// 断开串口连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (this.SerialPort != null && this.SerialPort.IsOpen)
                {
                    this.SerialPort.DataReceived -= SerialPort_DataReceived;
                    this.SerialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    this.SerialPort.PinChanged -= SerialPort_PinChanged;
                    this.SerialPort.Close();
                    this.SerialPort.Dispose();
                    this.SerialPort = null;
                }
                IsConnected = false;
            }
            catch (Exception ex)
            {
                
            }
        }


        /// <summary>
        /// 串口错误接收处理
        /// </summary>
        protected virtual void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
          
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
        protected virtual void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        /// <summary>
        /// 获取可用串口列表
        /// </summary>
        /// <returns>串口名称列表</returns>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 应用模版
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //this.PART_RSDialog = this.GetTemplateChild(nameof(this.PART_RSDialog)) as RSDialog;
            //this.PART_DataGrid = this.GetTemplateChild(nameof(this.PART_DataGrid)) as DataGrid;
            //this.PART_BtnConnect = this.GetTemplateChild(nameof(this.PART_BtnConnect)) as Button;
            //this.PART_BtnDisConnect = this.GetTemplateChild(nameof(this.PART_BtnDisConnect)) as Button;
            //this.PART_BtnSaveConfig = this.GetTemplateChild(nameof(this.PART_BtnSaveConfig)) as Button;

            //if (this.PART_BtnConnect != null)
            //{
            //    this.PART_BtnConnect.Click -= BtnConnect_Click;
            //    this.PART_BtnConnect.Click += BtnConnect_Click;
            //}

            //if (this.PART_BtnDisConnect != null)
            //{
            //    this.PART_BtnDisConnect.Click -= BtnDisConnect_Click;
            //    this.PART_BtnDisConnect.Click += BtnDisConnect_Click;
            //}

            //if (this.PART_BtnSaveConfig != null)
            //{
            //    this.PART_BtnSaveConfig.Click -= BtnSaveConfig_Click;
            //    this.PART_BtnSaveConfig.Click += BtnSaveConfig_Click;
            //}
        }

  

      

       
    }
}
