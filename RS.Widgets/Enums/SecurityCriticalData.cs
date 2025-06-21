using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Enums
{
    public struct SecurityCriticalData<T>
    {
        private T _value;

        internal T Value
        {
            get
            {
                return _value;
            }
        }

        internal SecurityCriticalData(T value)
        {
            _value = value;
        }
    }
}
