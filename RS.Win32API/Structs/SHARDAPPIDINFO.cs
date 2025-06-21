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
    public class SHARDAPPIDINFO
    {
        [MarshalAs(UnmanagedType.Interface)]
        object psi;    // The namespace location of the the item that should be added to the recent docs folder.
        [MarshalAs(UnmanagedType.LPWStr)]
        string pszAppID;  // The id of the application that should be associated with this recent doc.
    }
}
