using CommunityToolkit.Mvvm.ComponentModel;
using RS.Commons.Enums;
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

namespace RS.HMI.Client.DataConfigs
{
    /// <summary>
    /// Modbus数据配置
    /// </summary>
    public  class ModbusDataConfig : DataConfigBase
    {

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
                if (SetProperty(ref stationNumber, value))
                {
                    IsSaved = false;
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
                if (SetProperty(ref functionCode, value))
                {
                    IsSaved = false;
                }
                ValidProperty(value);
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
                SetProperty(ref byteOrder, value);
                ValidProperty(value);
            }
        }

    
        public override ModbusDataConfig Clone()
        {
            return new ModbusDataConfig()
            {
                Address = Address,
                ByteOrder = ByteOrder,
                CharacterLength = CharacterLength,
                DataDescription = DataDescription,
                DataGroup = DataGroup,
                DataId = DataId,
                DataType = DataType,
                DataValue = DataValue,
                FunctionCode = FunctionCode,
                ReadWritePermission = ReadWritePermission,
                StationNumber = StationNumber,
                IsSaved = IsSaved,
                IsValid = IsValid,
                IsStringInverse= IsStringInverse,
                DigitalNumber= DigitalNumber,
                MaxValue= MaxValue,
                MinValue= MinValue,
            };
        }
    }
}
