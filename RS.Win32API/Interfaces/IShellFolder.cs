using RS.Win32API.Enums;
using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using RS.Win32API.Structs;
namespace RS.Win32API.Interfaces
{

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellFolder),
    ]
    public interface IShellFolder
    {
        void ParseDisplayName(
            [In] IntPtr hwnd,
            [In] IBindCtx pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            [In, Out] ref int pchEaten,
            [Out] out IntPtr ppidl,
            [In, Out] ref uint pdwAttributes);

        IEnumIDList EnumObjects(
            [In] IntPtr hwnd,
            [In] SHCONTF grfFlags);

        // returns an instance of a sub-folder which is specified by the IDList (pidl).
        // IShellFolder or derived interfaces
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToObject(
            [In] IntPtr pidl,
            [In] IBindCtx pbc,
            [In] ref Guid riid);

        // produces the same result as BindToObject()
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToStorage([In] IntPtr pidl, [In] IBindCtx pbc, [In] ref Guid riid);

        // compares two IDLists and returns the result. The shell
        // explorer always passes 0 as lParam, which indicates 'sort by name'.
        // It should return 0 (as CODE of the scode), if two id indicates the
        // same object; negative value if pidl1 should be placed before pidl2;
        // positive value if pidl2 should be placed before pidl1.
        // use the macro ResultFromShort() to extract the result comparison
        // it deals with the casting and type conversion issues for you
        [PreserveSig]
        HRESULT CompareIDs([In] IntPtr lParam, [In] IntPtr pidl1, [In] IntPtr pidl2);

        // creates a view object of the folder itself. The view
        // object is a difference instance from the shell folder object.
        // 'hwndOwner' can be used  as the owner window of its dialog box or
        // menu during the lifetime of the view object.
        // This member function should always create a new
        // instance which has only one reference count. The explorer may create
        // more than one instances of view object from one shell folder object
        // and treat them as separate instances.
        // returns IShellView derived interface
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid);

        // returns the attributes of specified objects in that
        // folder. 'cidl' and 'apidl' specifies objects. 'apidl' contains only
        // simple IDLists. The explorer initializes *prgfInOut with a set of
        // flags to be evaluated. The shell folder may optimize the operation
        // by not returning unspecified flags.
        void GetAttributesOf(
            [In] uint cidl,
            [In] IntPtr apidl,
            [In, Out] ref SFGAO rgfInOut);

        // creates a UI object to be used for specified objects.
        // The shell explorer passes either IID_IDataObject (for transfer operation)
        // or IID_IContextMenu (for context menu operation) as riid
        // and many other interfaces
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetUIObjectOf(
            [In] IntPtr hwndOwner,
            [In] uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 2)] IntPtr apidl,
            [In] ref Guid riid,
            [In, Out] ref uint rgfReserved);

        // returns the display name of the specified object.
        // If the ID contains the display name (in the locale character set),
        // it returns the offset to the name. Otherwise, it returns a pointer
        // to the display name string (UNICODE), which is allocated by the
        // task allocator, or fills in a buffer.
        // use the helper APIS StrRetToStr() or StrRetToBuf() to deal with the different
        // forms of the STRRET structure
        void GetDisplayNameOf([In] IntPtr pidl, [In] SHGDN uFlags, [Out] out IntPtr pName);

        // sets the display name of the specified object.
        // If it changes the ID as well, it returns the new ID which is
        // alocated by the task allocator.
        void SetNameOf([In] IntPtr hwnd,
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In] SHGDN uFlags,
            [Out] out IntPtr ppidlOut);
    }
}
