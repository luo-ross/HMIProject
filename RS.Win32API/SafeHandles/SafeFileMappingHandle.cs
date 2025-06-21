using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {

        public SafeFileMappingHandle(IntPtr handle) : base(false)
        {
            SetHandle(handle);
        }


        public SafeFileMappingHandle() : base(true)
        {
        }


        public override bool IsInvalid
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return handle == IntPtr.Zero;
            }
        }


        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandleNoThrow(new HandleRef(null, handle));
        }
    }
}
