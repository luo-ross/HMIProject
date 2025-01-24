using RS.Widgets.Common.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public  class DeviceDataModel : NotifyBase
    {

        private int dataId;
        /// <summary>
        /// 数据标签 上位机使用
        /// </summary>
        public int DataId
        {
            get
            {
                return dataId;
            }
            set
            {
                OnPropertyChanged(ref dataId, value);
            }
        }


        private byte stationNumber;
        /// <summary>
        /// 设备地址，用于唯一标识网络中的一个设备，范围一般在 1 到 247 之间，0 通常作为广播地址
        /// </summary>
        public byte StationNumber
        {
            get
            {
                return stationNumber;
            }
            set
            {
                OnPropertyChanged(ref stationNumber, value);
            }
        }

        private FunctionCodeEnum functionCode= FunctionCodeEnum.ReadHoldingRegisters_0x03;
        /// <summary>
        /// 功能码，定义主站请求从站执行的操作类型，不同的功能码对应不同的操作
        /// </summary>
        public FunctionCodeEnum FunctionCode
        {
            get
            {
                return functionCode;
            }
            set
            {
                OnPropertyChanged(ref functionCode, value);
            }
        }

        private string address;
        /// <summary>
        /// 地址，表示要操作的数据或设备内部存储单元的位置，例如在 Modbus 协议中可以是寄存器或线圈的地址
        /// </summary>
        [Required(ErrorMessage = "读取地址不能为空")]
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                OnPropertyChanged(ref address, value);
                this.ValidProperty(value);
            }
        }

        private DataTypeEnum dataType = DataTypeEnum.Boolean;
        /// <summary>
        /// 数据类型，
        /// </summary>
        public DataTypeEnum DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                OnPropertyChanged(ref dataType, value);
            }
        }

        private int? characterLength;
        /// <summary>
        /// 字符长度，对于不同的数据类型，其表示字符长度可能不同，如对于 ushort 是 2 字节
        /// </summary>
        public int? CharacterLength
        {
            get
            {
                return characterLength;
            }
            set
            {
                OnPropertyChanged(ref characterLength, value);
            }
        }

        private ReadWriteEnum readWritePermission = ReadWriteEnum.ReadWrite;
        /// <summary>
        /// 读写权限，使用枚举表示，可根据实际需求自定义枚举，如 Read、Write、ReadWrite
        /// </summary>
        public ReadWriteEnum ReadWritePermission
        {
            get
            {
                return readWritePermission;
            }
            set
            {
                OnPropertyChanged(ref readWritePermission, value);
            }
        }

        private byte dataGroup;
        /// <summary>
        /// 数据分组，可用于对数据进行分组管理，例如可以将不同功能的数据分为不同的组
        /// </summary>
        public byte DataGroup
        {
            get
            {
                return dataGroup;
            }
            set
            {
                OnPropertyChanged(ref dataGroup, value);
            }
        }

        private string dataDescription;
        /// <summary>
        /// 数据描述，对该数据的具体描述，用于说明该数据在通信中的作用和用途
        /// </summary>
        [Required(ErrorMessage = "数据描述不能为空")]
        public string DataDescription
        {
            get
            {
                return dataDescription;
            }
            set
            {
                OnPropertyChanged(ref dataDescription, value);
                this.ValidProperty(value);
            }
        }

        private ByteOrderEnum byteOrder = ByteOrderEnum.ABCD;
        /// <summary>
        /// 字节序
        /// </summary>
        public ByteOrderEnum ByteOrder
        {
            get
            {
                return byteOrder;
            }
            set
            {
                this.OnPropertyChanged(ref byteOrder, value);
            }
        }


        private double? dataValue;
        /// <summary>
        /// 数据值
        /// </summary>
        public double? DataValue
        {
            get
            {
                return dataValue;
            }
            private set
            {
                this.OnPropertyChanged(ref dataValue, value);
            }
        }

        /// <summary>
        /// 这里是手动克隆不叨叨了肯定是最快的
        /// 想用其克隆方式，可以去Nuget安装什么浅拷贝或者深拷贝啥的
        /// </summary>
        /// <returns></returns>
        public DeviceDataModel Clone()
        {
            return new DeviceDataModel()
            {
                Address = this.Address,
                ByteOrder = this.ByteOrder,
                CharacterLength = this.CharacterLength,
                DataDescription = this.DataDescription,
                DataGroup = this.DataGroup,
                DataId = this.DataId,
                DataType = this.DataType,
                DataValue = this.DataValue,
                FunctionCode = this.FunctionCode,
                ReadWritePermission = this.ReadWritePermission,
                StationNumber = this.StationNumber,
            };
        }


    }
}
