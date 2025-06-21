using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [Serializable]
    public struct SecurityCriticalDataForSet<T>
    {
        private T _value;

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public SecurityCriticalDataForSet(T value)
        {
            _value = value;
        }
    }
}
