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
    /// Provides access to the App User Model ID on objects supporting this value.
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectWithAppUserModelId)
    ]
    public interface IObjectWithAppUserModelId
    {
        void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetAppID();
    };
}
