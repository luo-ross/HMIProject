using RS.Commons.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.DataConfigs
{
    public abstract class DataConfigBase : NotifyBase
    {

        /// <summary>
        /// 数据唯一主键
        /// </summary>
        public string Id { get; set; }


        private string? dataId;
        /// <summary>
        /// 数据标签 上位机使用
        /// </summary>
        [Required(ErrorMessage = "数据标签不能为空")]
        public string? DataId
        {
            get
            {
                return dataId;
            }
            set
            {
                if (SetProperty(ref dataId, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref dataType, value))
                {
                    IsSaved = false;
                    //判断数据类型是否是String类型
                    if (dataType != DataTypeEnum.String)
                    {
                        CharacterLength = null;
                    }
                    else
                    {
                        CharacterLength = 10;
                    }
                }
                ValidProperty(value);
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
                if (SetProperty(ref characterLength, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref isStringInverse, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref readWritePermission, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref dataGroup, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref dataDescription, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
            }
        }



        private string? address;
        /// <summary>
        /// 地址，表示要操作的数据或设备内部存储单元的位置，例如在 Modbus 协议中可以是寄存器或线圈的地址
        /// </summary>
        public string? Address
        {
            get
            {
                return address;
            }
            set
            {
                if (SetProperty(ref address, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
            set
            {
                SetProperty(ref dataValue, value);
                ValidProperty(value);
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
                if (SetProperty(ref minValue, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                if (SetProperty(ref maxValue, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                SetProperty(ref digitalNumber, value);
                ValidProperty(value);
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
                SetProperty(ref isValid, value);
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
        public abstract DataConfigBase Clone();
    }
}
