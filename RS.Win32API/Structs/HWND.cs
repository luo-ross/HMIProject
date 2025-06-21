using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    public struct HWND
    {
        public IntPtr h;

        public static HWND Cast(IntPtr h)
        {
            HWND result = default(HWND);
            result.h = h;
            return result;
        }

        public HandleRef MakeHandleRef(object wrapper)
        {
            return new HandleRef(wrapper, h);
        }

        public static implicit operator IntPtr(HWND h)
        {
            return h.h;
        }

        public static bool operator ==(HWND hl, HWND hr)
        {
            return hl.h == hr.h;
        }

        public static bool operator !=(HWND hl, HWND hr)
        {
            return hl.h != hr.h;
        }

        public override bool Equals(object oCompare)
        {
            HWND hWND = Cast((HWND)oCompare);
            return h == hWND.h;
        }

        public override int GetHashCode()
        {
            return (int)h;
        }
    }

}
