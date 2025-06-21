using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PKEY
    {
        /// <summary>fmtid</summary>
        private readonly Guid _fmtid;
        /// <summary>pid</summary>
        private readonly uint _pid;

        public PKEY(Guid fmtid, uint pid)
        {
            _fmtid = fmtid;
            _pid = pid;
        }

        /// <summary>PKEY_Title</summary>
        public static readonly PKEY Title = new PKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        /// <summary>PKEY_AppUserModel_ID</summary>
        public static readonly PKEY AppUserModel_ID = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
        /// <summary>PKEY_AppUserModel_IsDestListSeparator</summary>
        public static readonly PKEY AppUserModel_IsDestListSeparator = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);
        /// <summary>PKEY_AppUserModel_RelaunchCommand</summary>
        public static readonly PKEY AppUserModel_RelaunchCommand = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);
        /// <summary>PKEY_AppUserModel_RelaunchDisplayNameResource</summary>
        public static readonly PKEY AppUserModel_RelaunchDisplayNameResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);
        /// <summary>PKEY_AppUserModel_RelaunchIconResource</summary>
        public static readonly PKEY AppUserModel_RelaunchIconResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
    }
}
