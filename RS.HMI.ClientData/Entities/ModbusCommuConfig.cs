using RS.Commons.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.ClientData.Entities
{

    /// <summary>
    /// Modbus通讯配置
    /// </summary>
    public sealed class ModbusCommuConfig
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属通讯站 Id
        /// </summary>
        public string CommuStationId { get; set; }

        /// <summary>
        /// 所属通讯配置主键
        /// </summary>
        public string SerialPortConfigId { get; set; }
    
        /// <summary>
        /// 数据标签 上位机使用
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// 设备地址，用于唯一标识网络中的一个设备，范围一般在 1 到 247 之间，0 通常作为广播地址
        /// </summary>
        public byte StationNumber { get; set; }

        /// <summary>
        /// 功能码，定义主站请求从站执行的操作类型，不同的功能码对应不同的操作
        /// </summary>
        public FunctionCodeEnum FunctionCode { get; set; }

        /// <summary>
        /// 地址，表示要操作的数据或设备内部存储单元的位置，例如在 Modbus 协议中可以是寄存器或线圈的地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeEnum DataType { get; set; }

        /// <summary>
        /// 字符长度，对于不同的数据类型，其表示字符长度可能不同，如对于 ushort 是 2 字节
        /// </summary>
        public int? CharacterLength { get; set; }

        /// <summary>
        /// 是否字符串颠倒
        /// </summary>
        public bool? IsStringInverse { get; set; }

        /// <summary>
        /// 读写权限，使用枚举表示，可根据实际需求自定义枚举，如 Read、ReadWrite
        /// </summary>
        public ReadWriteEnum ReadWritePermission { get; set; }

        /// <summary>
        /// 数据分组，可用于对数据进行分组管理，例如可以将不同功能的数据分为不同的组
        /// </summary>
        public byte DataGroup { get; set; }

        /// <summary>
        /// 数据描述，对该数据的具体描述，用于说明该数据在通信中的作用和用途
        /// </summary>
        /// <remarks>
        /// 该属性不能为空，否则会触发 Required 验证错误
        /// </remarks>
        public string DataDescription { get; set; }

        /// <summary>
        /// 字节序
        /// </summary>
        public ByteOrderEnum ByteOrder { get; set; }

        /// <summary>
        /// 数据值
        /// </summary>
        public double? DataValue { get; private set; }

        /// <summary>
        /// 读写最小值
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// 读写最大值
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// 如果是浮点数，保留小数点位数
        /// </summary>
        public byte? DigitalNumber { get; set; }
     
    }
}
