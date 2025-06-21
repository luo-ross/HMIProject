using Microsoft.Win32.SafeHandles;
using RS.Win32API.Structs;
using System.Security;

namespace RS.Win32API.SafeHandles
{
    public sealed class SafeDC : SafeHandleZeroOrMinusOneIsInvalid
    {
     
        private IntPtr? _hwnd;

        private bool _created;

        public IntPtr Hwnd
        {
            [SecurityCritical]
            set
            {
                _hwnd = value;
            }
        }

        private SafeDC()
            : base(ownsHandle: true)
        {
        }

       
        protected override bool ReleaseHandle()
        {
            if (_created)
            {
                return NativeMethods.DeleteDC(handle);
            }

            if (!_hwnd.HasValue || _hwnd.Value == IntPtr.Zero)
            {
                return true;
            }

            return NativeMethods.ReleaseDC(_hwnd.Value, handle) == 1;
        }

        public static SafeDC CreateDC(string deviceName)
        {
            SafeDC safeDC = null;
            try
            {
                safeDC = NativeMethods.CreateDC(deviceName, null, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                if (safeDC != null)
                {
                    safeDC._created = true;
                }
            }

            if (safeDC.IsInvalid)
            {
                safeDC.Dispose();
                throw new SystemException("Unable to create a device context from the specified device information.");
            }

            return safeDC;
        }

        public static SafeDC CreateCompatibleDC(SafeDC hdc)
        {
            SafeDC safeDC = null;
            try
            {
                IntPtr zero = IntPtr.Zero;
                if (hdc != null)
                {
                    zero = hdc.handle;
                }

                safeDC = NativeMethods.CreateCompatibleDC(zero);
                if (safeDC == null)
                {
                    HRESULT.ThrowLastError();
                }
            }
            finally
            {
                if (safeDC != null)
                {
                    safeDC._created = true;
                }
            }

            if (safeDC.IsInvalid)
            {
                safeDC.Dispose();
                throw new SystemException("Unable to create a device context from the specified device information.");
            }

            return safeDC;
        }

        public static SafeDC GetDC(IntPtr hwnd)
        {
            SafeDC safeDC = null;
            try
            {
                safeDC = NativeMethods.GetDC(hwnd);
            }
            finally
            {
                if (safeDC != null)
                {
                    safeDC.Hwnd = hwnd;
                }
            }

            if (safeDC.IsInvalid)
            {
                HRESULT.E_FAIL.ThrowIfFailed();
            }

            return safeDC;
        }

        public static SafeDC GetDesktop()
        {
            return GetDC(IntPtr.Zero);
        }

        public static SafeDC WrapDC(IntPtr hdc)
        {
            return new SafeDC
            {
                handle = hdc,
                _created = false,
                _hwnd = IntPtr.Zero
            };
        }
    }
}
