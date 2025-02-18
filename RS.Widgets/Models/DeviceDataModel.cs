using RS.Widgets.Common.Enums;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace RS.Widgets.Models
{
    public class DeviceDataModel : NotifyBase
    {

        private int? dataId;
        /// <summary>
        /// 数据标签 上位机使用
        /// </summary>
        [Required(ErrorMessage = "数据标签不能为空")]
        public int? DataId
        {
            get
            {
                return dataId;
            }
            set
            {
                if (OnPropertyChanged(ref dataId, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
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
                if (OnPropertyChanged(ref stationNumber, value))
                {
                    this.IsSaved = false;
                }
            }
        }

        private FunctionCodeEnum functionCode = FunctionCodeEnum.ReadHoldingRegisters_0x03;
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
                if (OnPropertyChanged(ref functionCode, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }

        private int address;
        /// <summary>
        /// 地址，表示要操作的数据或设备内部存储单元的位置，例如在 Modbus 协议中可以是寄存器或线圈的地址
        /// </summary>
        public int Address
        {
            get
            {
                return address;
            }
            set
            {
                if (OnPropertyChanged(ref address, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }

        private DataTypeEnum dataType = DataTypeEnum.Bool;
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
                if (OnPropertyChanged(ref dataType, value))
                {
                    this.IsSaved = false;
                    //判断数据类型是否是String类型
                    if (dataType != DataTypeEnum.String)
                    {
                        this.CharacterLength = null;
                    }
                    else
                    {
                        this.CharacterLength = 10;
                    }
                }
                this.ValidProperty(value);
            }
        }


        #region 字符串读写相关设置


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
                if (OnPropertyChanged(ref characterLength, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }

        private bool? isStringInverse;
        /// <summary>
        /// 是否字符串颠倒
        /// </summary>

        public bool? IsStringInverse
        {
            get
            {
                return isStringInverse;
            }
            set
            {
                if (OnPropertyChanged(ref isStringInverse, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }
        #endregion



        private ReadWriteEnum readWritePermission = ReadWriteEnum.Read;
        /// <summary>
        /// 读写权限，使用枚举表示，可根据实际需求自定义枚举，如 Read、ReadWrite
        /// </summary>
        public ReadWriteEnum ReadWritePermission
        {
            get
            {
                return readWritePermission;
            }
            set
            {
                if (OnPropertyChanged(ref readWritePermission, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
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
                if (OnPropertyChanged(ref dataGroup, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
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
                if (OnPropertyChanged(ref dataDescription, value))
                {
                    this.IsSaved = false;
                }
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
                this.ValidProperty(value);
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
                this.ValidProperty(value);
            }
        }

        private double? minValue;
        /// <summary>
        /// 读写最大值
        /// </summary>
        public double? MinValue
        {
            get
            {
                return minValue;
            }
             set
            {
                if (OnPropertyChanged(ref minValue, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }


        private double? maxValue;
        /// <summary>
        /// 读写最大值
        /// </summary>
        public double? MaxValue
        {
            get
            {
                return maxValue;
            }
             set
            {
                if (OnPropertyChanged(ref maxValue, value))
                {
                    this.IsSaved = false;
                }
                this.ValidProperty(value);
            }
        }

        private byte? digitalNumber;
        /// <summary>
        /// 如果是浮点数，保留小数点位数
        /// </summary>
        public byte? DigitalNumber
        {
            get
            {
                return digitalNumber;
            }
            set
            {
                this.OnPropertyChanged(ref digitalNumber, value);
                this.ValidProperty(value);
            }
        }



        private bool isValid;
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                this.OnPropertyChanged(ref isValid, value);
            }
        }


        /// <summary>
        /// 记录当前数据是否保存
        /// </summary>
        public bool IsSaved { get; set; }

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
                IsSaved = this.IsSaved,
                IsValid = this.IsValid,
                IsStringInverse=this.IsStringInverse,
                DigitalNumber=this.DigitalNumber,
                MaxValue=this.MaxValue,
                MinValue=this.MinValue,
            };
        }


    }
}
