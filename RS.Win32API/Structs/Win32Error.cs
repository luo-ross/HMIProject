using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Win32Error
    {
        [FieldOffset(0)]
        private readonly int _value;

        public static readonly Win32Error ERROR_SUCCESS = new Win32Error(0);

        public static readonly Win32Error ERROR_INVALID_FUNCTION = new Win32Error(1);

        public static readonly Win32Error ERROR_FILE_NOT_FOUND = new Win32Error(2);

        public static readonly Win32Error ERROR_PATH_NOT_FOUND = new Win32Error(3);

        public static readonly Win32Error ERROR_TOO_MANY_OPEN_FILES = new Win32Error(4);

        public static readonly Win32Error ERROR_ACCESS_DENIED = new Win32Error(5);

        public static readonly Win32Error ERROR_INVALID_HANDLE = new Win32Error(6);

        public static readonly Win32Error ERROR_OUTOFMEMORY = new Win32Error(14);

        public static readonly Win32Error ERROR_NO_MORE_FILES = new Win32Error(18);

        public static readonly Win32Error ERROR_SHARING_VIOLATION = new Win32Error(32);

        public static readonly Win32Error ERROR_INVALID_PARAMETER = new Win32Error(87);

        public static readonly Win32Error ERROR_INSUFFICIENT_BUFFER = new Win32Error(122);

        public static readonly Win32Error ERROR_NESTING_NOT_ALLOWED = new Win32Error(215);

        public static readonly Win32Error ERROR_KEY_DELETED = new Win32Error(1018);

        public static readonly Win32Error ERROR_NO_MATCH = new Win32Error(1169);

        public static readonly Win32Error ERROR_BAD_DEVICE = new Win32Error(1200);

        public static readonly Win32Error ERROR_CANCELLED = new Win32Error(1223);

        public static readonly Win32Error ERROR_INVALID_WINDOW_HANDLE = new Win32Error(1400);

        public static readonly Win32Error ERROR_TIMEOUT = new Win32Error(1460);

        public static readonly Win32Error ERROR_INVALID_DATATYPE = new Win32Error(1804);

        public static readonly Win32Error ERROR_NOT_FOUND = new Win32Error(1168);
       
        public static readonly Win32Error ERROR_CLASS_ALREADY_EXISTS = new Win32Error(1410);


        public Win32Error(int i)
        {
            _value = i;
        }

        public static explicit operator HRESULT(Win32Error error)
        {
            if (error._value <= 0)
            {
                return new HRESULT((uint)error._value);
            }

            return HRESULT.Make(severe: true, Facility.Win32, error._value & 0xFFFF);
        }

        public HRESULT ToHRESULT()
        {
            return (HRESULT)this;
        }

        [SecurityCritical]
        public static Win32Error GetLastError()
        {
            return new Win32Error(Marshal.GetLastWin32Error());
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((Win32Error)obj)._value == _value;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(Win32Error errLeft, Win32Error errRight)
        {
            return errLeft._value == errRight._value;
        }

        public static bool operator !=(Win32Error errLeft, Win32Error errRight)
        {
            return !(errLeft == errRight);
        }

      
    }
}
