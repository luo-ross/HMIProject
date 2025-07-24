using System.Runtime.InteropServices;

namespace RS.Win32API.Standard
{
    public static  class SystemUtility
    {

        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }

        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }

        public static int HIWORD(int i)
        {
            return (short)(i >> 16);
        }

        public static int LOWORD(int i)
        {
            return (short)(i & 0xFFFF);
        }

        public static bool IsFlagSet(int value, int mask)
        {
            return 0 != (value & mask);
        }

        public static bool IsFlagSet(uint value, uint mask)
        {
            return 0 != (value & mask);
        }

        public static bool IsFlagSet(long value, long mask)
        {
            return 0 != (value & mask);
        }

        public static bool IsFlagSet(ulong value, ulong mask)
        {
            return 0 != (value & mask);
        }

        public static int MAKELONG(int low, int high)
        {
            return (high << 16) | (low & 0xffff);
        }

        public static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)((high << 16) | (low & 0xffff));
        }

        public static int HIWORD(IntPtr n)
        {
            return HIWORD(unchecked((int)(long)n));
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
