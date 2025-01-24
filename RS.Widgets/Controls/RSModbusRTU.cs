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

namespace RS.Widgets.Controls
{
    public class RSModbusRTU : RSSerialPort
    {
        static RSModbusRTU()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSModbusRTU), new FrameworkPropertyMetadata(typeof(RSModbusRTU)));
        }

        public RSModbusRTU()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
