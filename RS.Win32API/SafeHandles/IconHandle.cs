using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public sealed class IconHandle : WpfSafeHandle
    {
        /// <SecurityNote>
        /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
        /// </SecurityNote>
        private IconHandle() : base(true, CommonHandles.Icon)
        {
        }

        /// <SecurityNote>
        ///     Critical: This calls into DestroyIcon
        /// </SecurityNote>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.DestroyIcon(handle);
        }

        /// <SecurityNote>
        ///     Critical: This creates a new SafeHandle, which has a critical constructor.
        ///     TreatAsSafe: The handle this creates is invalid.  It contains no critical data.
        /// </SecurityNote>
        public static IconHandle GetInvalidIcon()
        {
            return new IconHandle();
        }

        /// <summary>
        /// Get access to the raw handle for native APIs that require it.
        /// </summary>
        /// <SecurityNote>
        ///     Critical: This accesses critical data for the safe handle.
        /// </SecurityNote>
        public nint CriticalGetHandle()
        {
            return handle;
        }
    }
}
