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

namespace RS.Win32API.Handles
{
    public sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <SecurityNote>
        ///   Critical: base class enforces link demand and inheritance demand
        /// </SecurityNote>
        [SecurityCritical]
        public SafeFileMappingHandle(IntPtr handle) : base(false)
        {
            SetHandle(handle);
        }

        /// <SecurityNote>
        ///   Critical: base class enforces link demand and inheritance demand
        ///   TreatAsSafe: Creating this is ok, accessing the pointer is bad
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public SafeFileMappingHandle() : base(true)
        {
        }

        /// <SecurityNote>
        ///   Critical: base class enforces link demand and inheritance demand
        ///   TreatAsSafe: This call is safe
        /// </SecurityNote>
        public override bool IsInvalid
        {
            [SecurityCritical, SecurityTreatAsSafe]
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        /// <SecurityNote>
        ///     Critical - as this function does an elevation to close a handle.
        ///     TreatAsSafe - as this can at best be used to destabilize one's own app.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        protected override bool ReleaseHandle()
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                return NativeMethods.CloseHandleNoThrow(new HandleRef(null, handle));
            }
            finally
            {
                SecurityPermission.RevertAssert();
            }
        }
    }
}
