using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class SHARDAPPIDINFOLINK
    {
        IntPtr psl;     // An IShellLink instance that when launched opens a recently used item in the specified
        // application. This link is not added to the recent docs folder, but will be added to the
        // specified application's destination list.
        [MarshalAs(UnmanagedType.LPWStr)]
        string pszAppID;  // The id of the application that should be associated with this recent doc.
    }
}
