using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{


    // Used to remove items from the automatic destination lists created when apps or the system call SHAddToRecentDocs to report usage of a document.
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ApplicationDestinations)
    ]
    public interface IApplicationDestinations
    {
        // Set the App User Model ID for the application removing destinations from its list.  If an AppID is not provided 
        // via this method, the system will use a heuristically determined ID.  This method must be called before
        // RemoveDestination or RemoveAllDestinations.
        void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        // Remove an IShellItem or an IShellLink from the automatic destination list
        void RemoveDestination([MarshalAs(UnmanagedType.IUnknown)] object punk);

        // Clear the frequent and recent destination lists for this application.
        void RemoveAllDestinations();
    }
}
