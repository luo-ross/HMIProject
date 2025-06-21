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

    /// <summary>
    /// Shell Namespace helper 2
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellItem2),
    ]
  public  interface IShellItem2 : IShellItem
    {
        #region IShellItem redeclarations
        [return: MarshalAs(UnmanagedType.Interface)]
        new object BindToHandler([In] IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);
        new IShellItem GetParent();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        new string GetDisplayName(SIGDN sigdnName);
        new SFGAO GetAttributes(SFGAO sfgaoMask);
        new int Compare(IShellItem psi, SICHINT hint);
        #endregion

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStore(
            GPS flags,
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreWithCreateObject(
            GPS flags,
            [MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject,   // factory for low-rights creation of type ICreateObject
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreForKeys(
            IntPtr rgKeys,
            uint cKeys,
            GPS flags,
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyDescriptionList(
            IntPtr keyType,
            [In] ref Guid riid);

        // Ensures any cached information in this item is up to date, or returns __HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) if the item does not exist.
        void Update(IBindCtx pbc);

        /// <SecurityNote>
        ///   Critical : Calls critical methods
        /// <SecurityNote>

        PROPVARIANT GetProperty(IntPtr key);

        Guid GetCLSID(IntPtr key);

        FILETIME GetFileTime(IntPtr key);

        int GetInt32(IntPtr key);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetString(IntPtr key);

        uint GetUInt32(IntPtr key);

        ulong GetUInt64(IntPtr key);

        [return: MarshalAs(UnmanagedType.Bool)]
        void GetBool(IntPtr key);
    }
}
