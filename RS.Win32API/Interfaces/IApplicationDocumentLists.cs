using RS.Win32API.Enums;
using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{

    /// <summary>
    /// Allows an application to retrieve the most recent and frequent documents opened in that app, as reported via SHAddToRecentDocs
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ApplicationDocumentLists)
    ]
    public interface IApplicationDocumentLists
    {
        /// <summary>
        /// Set the App User Model ID for the application retrieving this list.  If an AppID is not provided via this method,
        /// the system will use a heuristically determined ID.  This method must be called before GetList. 
        /// </summary>
        /// <param name="pszAppID">App Id.</param>
        void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        /// <summary>
        /// Retrieve an IEnumObjects or IObjectArray for IShellItems and/or IShellLinks. 
        /// Items may appear in both the frequent and recent lists.  
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetList([In] APPDOCLISTTYPE listtype, [In] uint cItemsDesired, [In] ref Guid riid);
    }
}
