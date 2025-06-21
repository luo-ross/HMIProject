using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using RS.Win32API;

namespace RS.Win32API.SafeHandles
{
    public sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <SecurityNote>
        ///   Critical : Calls critical ctor
        /// </SecurityNote>
        private SafeFindHandle() : base(true) { }

        /// <SecurityNote>
        ///   Critical : Base method is critical
        /// </SecurityNote>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FindClose(handle);
        }
    }
}
