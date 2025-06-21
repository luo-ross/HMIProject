using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public sealed class CursorHandle : WpfSafeHandle
    {
     
        private CursorHandle()
            : base(ownsHandle: true, CommonHandles.Cursor)
        {
        }
        
        protected override bool ReleaseHandle()
        {
            return NativeMethods.IntDestroyCursor(handle);
        }

      
        public static CursorHandle GetInvalidCursor()
        {
            return new CursorHandle();
        }
    }

}
