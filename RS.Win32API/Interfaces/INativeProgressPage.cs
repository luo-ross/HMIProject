using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{
    /// <SecurityNote>
    /// Critical due to SUC. 
    /// Even if a partilar method is considered safe, applying [SecurityTreatAsSafe] to it here won't help 
    /// much, because the transparency model still requires SUC-d methods to be called only from 
    /// SecurityCritical ones.
    /// </SecurityNote>
    [ComImport, Guid("1f681651-1024-4798-af36-119bbe5e5665")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INativeProgressPage
    {
        [PreserveSig]
        HRESULT Show();
        [PreserveSig]
        HRESULT Hide();
        [PreserveSig]
        HRESULT ShowProgressMessage(string message);
        [PreserveSig]
        HRESULT SetApplicationName(string appName);
        [PreserveSig]
        HRESULT SetPublisherName(string publisherName);
        [PreserveSig]
        HRESULT OnDownloadProgress(ulong bytesDownloaded, ulong bytesTotal);
    };
}
