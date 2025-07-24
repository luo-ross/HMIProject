using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{
    [ComImport, Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternetSecurityMgrSite
    {
        void GetWindow( /* [out] */ ref IntPtr phwnd);
        void EnableModeless( /* [in] */ bool fEnable);
    }
}
