using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.EnumIdList),
    ]
    public interface IEnumIDList
    {
        [PreserveSig()]
        HRESULT Next(uint celt, out IntPtr rgelt, out int pceltFetched);
        [PreserveSig()]
        HRESULT Skip(uint celt);
        void Reset();
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
    }
}
