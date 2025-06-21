using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Structs
{
    public struct _SystemParameterBoundProperty
    {
        public string SystemParameterPropertyName { get; set; }

        public DependencyProperty DependencyProperty { get; set; }
    }
}
