using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.ClientData.Entities
{
    /// <summary>
    /// 串口通讯配置
    /// </summary>
    public sealed class SerialPortConfig
    {
        /// <summary>
        /// 数据库主键
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 所属通讯站 Id
        /// </summary>
        public string CommuStationId { get; set; }

        /// <summary>
        /// 获取或设置串口的名称，例如 "COM1", "COM2" 等。
        /// </summary>
        public string? PortName { get; set; }

        /// <summary>
        /// 获取或设置串口通信的波特率，常见值如 9600, 115200 等。
        /// 波特率决定了数据传输的速率。
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 获取或设置每个字节中数据位的数量，通常为 7 或 8。
        /// 数据位表示一个字节中用于传输实际数据的位数。
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// 获取或设置串口通信的停止位。
        /// 停止位用于标识一个字节传输的结束。
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 获取或设置串口通信的奇偶校验位。
        /// 奇偶校验用于检测数据传输过程中是否发生错误。
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否自动进行串口连接。
        /// 如果设置为 true，则在满足条件时会自动尝试连接串口；否则需要手动触发连接操作。
        /// </summary>
        public bool IsAutoConnect { get; set; }

    }
}
