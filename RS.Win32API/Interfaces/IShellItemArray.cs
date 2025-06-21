using RS.Win32API.Enums;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
namespace RS.Win32API.Interfaces
{
    [
       ComImport,
       InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
       Guid(IID.ShellItemArray),
   ]
    public interface IShellItemArray
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToHandler(IBindCtx pbc, [In] ref Guid rbhid, [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStore(int flags, [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyDescriptionList([In] ref PKEY keyType, [In] ref Guid riid);

        uint GetAttributes(SIATTRIBFLAGS dwAttribFlags, uint sfgaoMask);

        uint GetCount();

        IShellItem GetItemAt(uint dwIndex);

        [return: MarshalAs(UnmanagedType.Interface)]
        object EnumItems();
    }
}
