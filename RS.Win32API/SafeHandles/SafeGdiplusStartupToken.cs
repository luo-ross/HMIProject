using Microsoft.Win32.SafeHandles;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public sealed class SafeGdiplusStartupToken : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <SecurityNote>
        ///   Critical : Calls critical ctor
        /// </SecurityNote>
        private SafeGdiplusStartupToken() : base(true) { }

        /// <SecurityNote>
        ///   Critical : Base member is critical
        /// </SecurityNote>
        protected override bool ReleaseHandle()
        {
            Status s = NativeMethods.GdiplusShutdown(this.handle);
            return s == Status.Ok;
        }

        /// <SecurityNote>
        ///   Critical : Call critical methods
        /// </SecurityNote>
        [SecurityCritical]
        public static SafeGdiplusStartupToken Startup()
        {
            SafeGdiplusStartupToken safeHandle = new SafeGdiplusStartupToken();
            IntPtr unsafeHandle;
            StartupOutput output;
            Status s = NativeMethods.GdiplusStartup(out unsafeHandle, new StartupInput(), out output);
            if (s == Status.Ok)
            {
                safeHandle.handle = unsafeHandle;
                return safeHandle;
            }
            safeHandle.Dispose();
            throw new Exception("Unable to initialize GDI+");
        }
    }
}
