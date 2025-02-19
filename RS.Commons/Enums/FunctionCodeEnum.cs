using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Enums
{
    public enum FunctionCodeEnum
    {
        /// <summary>
        /// 读线圈（单个或多个），功能码 01
        /// </summary>
        ReadCoils_0x01 = 0x01,

        /// <summary>
        /// 读离散输入（单个或多个），功能码 02
        /// </summary>
        ReadDiscreteInputs_0x02 = 0x02,

        /// <summary>
        /// 读保持寄存器（单个或多个），功能码 03
        /// </summary>
        ReadHoldingRegisters_0x03 = 0x03,

        /// <summary>
        /// 读输入寄存器（单个或多个），功能码 04
        /// </summary>
        ReadInputRegisters_0x04 = 0x04,

        /// <summary>
        /// 写单个线圈，功能码 05
        /// </summary>
        WriteSingleCoil_0x05 = 0x05,

        /// <summary>
        /// 写单个保持寄存器，功能码 06
        /// </summary>
        WriteSingleRegister_0x06 = 0x06,

        /// <summary>
        /// 读异常状态（仅用于串口 RTU 协议），功能码 07
        /// </summary>
        ReadExceptionStatus_0x07 = 0x07,

        /// <summary>
        /// 诊断，功能码 08
        /// </summary>
        Diagnostic_0x08 = 0x08,

        /// <summary>
        /// 编程（仅用于串口 ASCII 协议），功能码 11
        /// </summary>
        Program_0x0B = 0x0B,

        /// <summary>
        /// 读取设备标识，功能码 17
        /// </summary>
        ReadDeviceIdentification_0x11 = 0x11,

        /// <summary>
        /// 写多个线圈，功能码 15
        /// </summary>
        WriteMultipleCoils_0x0F = 0x0F,

        /// <summary>
        /// 写多个保持寄存器，功能码 16
        /// </summary>
        WriteMultipleRegisters_0x10 = 0x10,

        /// <summary>
        /// 报告从站 ID，功能码 17（用于串口 RTU 协议）
        /// </summary>
        ReportSlaveId_0x11 = 0x11,

        /// <summary>
        /// 掩码写寄存器，功能码 22
        /// </summary>
        MaskWriteRegister_0x16 = 0x16,

        /// <summary>
        /// 读/写多个寄存器，功能码 23
        /// </summary>
        ReadWriteMultipleRegisters_0x17 = 0x17,

        /// <summary>
        /// 读 FIFO 队列，功能码 24
        /// </summary>
        ReadFifoQueue_0x18 = 0x18
    }
}
