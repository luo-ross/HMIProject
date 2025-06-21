using RS.Win32API.Enums;
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


    // Custom Destination List
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.CustomDestinationList)
    ]
    public interface ICustomDestinationList
    {
        void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        // Retrieve IObjectArray of IShellItems or IShellLinks that represent removed destinations
        [return: MarshalAs(UnmanagedType.Interface)]
        object BeginList(out uint pcMaxSlots, [In] ref Guid riid);

        // PreserveSig because this will return custom errors when attempting to add unregistered ShellItems.
        // Can't readily detect that case without just trying to append it.
        [PreserveSig]
        HRESULT AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string pszCategory, IObjectArray poa);
        void AppendKnownCategory(KDC category);
        [PreserveSig]
        HRESULT AddUserTasks(IObjectArray poa);
        void CommitList();

        // Retrieve IObjectCollection of IShellItems
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetRemovedDestinations([In] ref Guid riid);
        void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
        void AbortList();
    }
}
