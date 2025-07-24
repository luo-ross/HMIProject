using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PROFILE
    {
        public ProfileType dwType; // profile type

        /// <SecurityNote>
        ///     Critical: Pointer field.
        /// </SecurityNote>

        public void* pProfileData;         // either the filename of the profile or buffer containing profile depending upon dwtype
        public uint cbDataSize;           // size in bytes of pProfileData
    };

}
