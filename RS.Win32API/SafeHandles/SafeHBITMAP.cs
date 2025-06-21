using Microsoft.Win32.SafeHandles;
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
    public sealed class SafeHBITMAP : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <SecurityNote>
        ///   Critical : Calls critical ctor
        /// </SecurityNote>
    
        private SafeHBITMAP() : base(true) { }

        /// <SecurityNote>
        ///   Critical : Base member is critical
        /// </SecurityNote>
      
        protected override bool ReleaseHandle()
        {
            return NativeMethods.DeleteObject(handle);
        }
    }
}
