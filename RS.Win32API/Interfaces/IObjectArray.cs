using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{

    /// <summary>Unknown Object Array</summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectArray),
    ]
    public interface IObjectArray
    {
        uint GetCount();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetAt([In] uint uiIndex, [In] ref Guid riid);
    }
}
