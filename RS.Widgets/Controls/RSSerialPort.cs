using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSSerialPort : ContentControl
    {
        static RSSerialPort()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSerialPort), new FrameworkPropertyMetadata(typeof(RSSerialPort)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
