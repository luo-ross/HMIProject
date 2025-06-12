using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Handles
{
    public sealed class IconHandle : WpfSafeHandle
    {
        /// <SecurityNote>
        /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
        /// </SecurityNote>
        [SecurityCritical]
        private IconHandle() : base(true, CommonHandles.Icon)
        {
        }

        /// <SecurityNote>
        ///     Critical: This calls into DestroyIcon
        /// </SecurityNote>
        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.DestroyIcon(handle);
        }

        /// <SecurityNote>
        ///     Critical: This creates a new SafeHandle, which has a critical constructor.
        ///     TreatAsSafe: The handle this creates is invalid.  It contains no critical data.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
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
        [SecurityCritical]
        public nint CriticalGetHandle()
        {
            return handle;
        }
    }
}
