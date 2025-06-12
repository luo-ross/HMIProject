using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API
{
    public static class Util
    {
        public static int MAKELONG(int low, int high)
        {
            return (high << 16) | (low & 0xffff);
        }

        public static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)((high << 16) | (low & 0xffff));
        }

        public static int HIWORD(int n)
        {
            return (n >> 16) & 0xffff;
        }

        public static int HIWORD(IntPtr n)
        {
            return HIWORD(unchecked((int)(long)n));
        }

        public static int LOWORD(int n)
        {
            return n & 0xffff;
        }

        public static int LOWORD(IntPtr n)
        {
            return LOWORD(unchecked((int)(long)n));
        }

        public static int SignedHIWORD(IntPtr n)
        {
            return SignedHIWORD(unchecked((int)(long)n));
        }
        public static int SignedLOWORD(IntPtr n)
        {
            return SignedLOWORD(unchecked((int)(long)n));
        }

        public static int SignedHIWORD(int n)
        {
            int i = (int)(short)((n >> 16) & 0xffff);

            return i;
        }

        public static int SignedLOWORD(int n)
        {
            int i = (int)(short)(n & 0xFFFF);

            return i;
        }

        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.Util.GetPInvokeStringLength"]/*' />
        /// <devdoc>
        ///     Computes the string size that should be passed to a typical Win32 call.
        ///     This will be the character count under NT, and the ubyte count for Windows 95.
        /// </devdoc>
        public static int GetPInvokeStringLength(String s)
        {
            if (s == null)
            {
                return 0;
            }

            if (Marshal.SystemDefaultCharSize == 2)
            {
                return s.Length;
            }
            else
            {
                if (s.Length == 0)
                {
                    return 0;
                }
                if (s.IndexOf('\0') > -1)
                {
                    return GetEmbeddedNullStringLengthAnsi(s);
                }
                else
                {
                    return NativeMethods.lstrlen(s);
                }
            }
        }

        private static int GetEmbeddedNullStringLengthAnsi(String s)
        {
            int n = s.IndexOf('\0');
            if (n > -1)
            {
                String left = s.Substring(0, n);
                String right = s.Substring(n + 1);
                return GetPInvokeStringLength(left) + GetEmbeddedNullStringLengthAnsi(right) + 1;
            }
            else
            {
                return GetPInvokeStringLength(s);
            }
        }



    }

}
