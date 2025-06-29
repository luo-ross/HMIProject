using RS.Widgets.Structs;
using RS.Win32API;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Controls
{
    public static class OSVersionHelper
    {
        public static bool IsOsWindows10RS5OrGreater { get; set; }

        public static bool IsOsWindows10RS4OrGreater { get; set; }

        public static bool IsOsWindows10RS3OrGreater { get; set; }

        public static bool IsOsWindows10RS2OrGreater { get; set; }

        public static bool IsOsWindows10RS1OrGreater { get; set; }

        public static bool IsOsWindows10TH2OrGreater { get; set; }

        public static bool IsOsWindows10TH1OrGreater { get; set; }

        public static bool IsOsWindows10OrGreater { get; set; }

        public static bool IsOsWindows8Point1OrGreater { get; set; }

        public static bool IsOsWindows8OrGreater { get; set; }

        public static bool IsOsWindows7SP1OrGreater { get; set; }

        public static bool IsOsWindows7OrGreater { get; set; }

        public static bool IsOsWindowsVistaSP2OrGreater { get; set; }

        public static bool IsOsWindowsVistaSP1OrGreater { get; set; }

        public static bool IsOsWindowsVistaOrGreater { get; set; }

        public static bool IsOsWindowsXPSP3OrGreater { get; set; }

        public static bool IsOsWindowsXPSP2OrGreater { get; set; }

        public static bool IsOsWindowsXPSP1OrGreater { get; set; }

        public static bool IsOsWindowsXPOrGreater { get; set; }

        public static bool IsOsWindowsServer { get; set; }


        static OSVersionHelper()
        {
            IsOsWindows10RS5OrGreater = NativeMethods.IsWindows10RS5OrGreater();
            IsOsWindows10RS4OrGreater = NativeMethods.IsWindows10RS4OrGreater();
            IsOsWindows10RS3OrGreater = NativeMethods.IsWindows10RS3OrGreater();
            IsOsWindows10RS2OrGreater = NativeMethods.IsWindows10RS2OrGreater();
            IsOsWindows10RS1OrGreater = NativeMethods.IsWindows10RS1OrGreater();
            IsOsWindows10TH2OrGreater = NativeMethods.IsWindows10TH2OrGreater();
            IsOsWindows10TH1OrGreater = NativeMethods.IsWindows10TH1OrGreater();
            IsOsWindows10OrGreater = NativeMethods.IsWindows10OrGreater();
            IsOsWindows8Point1OrGreater = NativeMethods.IsWindows8Point1OrGreater();
            IsOsWindows8OrGreater = NativeMethods.IsWindows8OrGreater();
            IsOsWindows7SP1OrGreater = NativeMethods.IsWindows7SP1OrGreater();
            IsOsWindows7OrGreater = NativeMethods.IsWindows7OrGreater();
            IsOsWindowsVistaSP2OrGreater = NativeMethods.IsWindowsVistaSP2OrGreater();
            IsOsWindowsVistaSP1OrGreater = NativeMethods.IsWindowsVistaSP1OrGreater();
            IsOsWindowsVistaOrGreater = NativeMethods.IsWindowsVistaOrGreater();
            IsOsWindowsXPSP3OrGreater = NativeMethods.IsWindowsXPSP3OrGreater();
            IsOsWindowsXPSP2OrGreater = NativeMethods.IsWindowsXPSP2OrGreater();
            IsOsWindowsXPSP1OrGreater = NativeMethods.IsWindowsXPSP1OrGreater();
            IsOsWindowsXPOrGreater = NativeMethods.IsWindowsXPOrGreater();
            IsOsWindowsServer = NativeMethods.IsWindowsServer();
        }

        public static bool IsOsVersionOrGreater(OperatingSystemVersion osVer)
        {
            return osVer switch
            {
                OperatingSystemVersion.Windows10RS5 => IsOsWindows10RS5OrGreater,
                OperatingSystemVersion.Windows10RS4 => IsOsWindows10RS4OrGreater,
                OperatingSystemVersion.Windows10RS3 => IsOsWindows10RS3OrGreater,
                OperatingSystemVersion.Windows10RS2 => IsOsWindows10RS2OrGreater,
                OperatingSystemVersion.Windows10RS1 => IsOsWindows10RS1OrGreater,
                OperatingSystemVersion.Windows10TH2 => IsOsWindows10TH2OrGreater,
                OperatingSystemVersion.Windows10 => IsOsWindows10OrGreater,
                OperatingSystemVersion.Windows8Point1 => IsOsWindows8Point1OrGreater,
                OperatingSystemVersion.Windows8 => IsOsWindows8OrGreater,
                OperatingSystemVersion.Windows7SP1 => IsOsWindows7SP1OrGreater,
                OperatingSystemVersion.Windows7 => IsOsWindows7OrGreater,
                OperatingSystemVersion.WindowsVistaSP2 => IsOsWindowsVistaSP2OrGreater,
                OperatingSystemVersion.WindowsVistaSP1 => IsOsWindowsVistaSP1OrGreater,
                OperatingSystemVersion.WindowsVista => IsOsWindowsVistaOrGreater,
                OperatingSystemVersion.WindowsXPSP3 => IsOsWindowsXPSP3OrGreater,
                OperatingSystemVersion.WindowsXPSP2 => IsOsWindowsXPSP2OrGreater,
                _ => throw new ArgumentException($"{osVer.ToString()} is not a valid OS!", "osVer"),
            };
        }

        public static OperatingSystemVersion GetOsVersion()
        {
            if (IsOsWindows10RS5OrGreater)
            {
                return OperatingSystemVersion.Windows10RS5;
            }

            if (IsOsWindows10RS4OrGreater)
            {
                return OperatingSystemVersion.Windows10RS4;
            }

            if (IsOsWindows10RS3OrGreater)
            {
                return OperatingSystemVersion.Windows10RS3;
            }

            if (IsOsWindows10RS2OrGreater)
            {
                return OperatingSystemVersion.Windows10RS2;
            }

            if (IsOsWindows10RS1OrGreater)
            {
                return OperatingSystemVersion.Windows10RS1;
            }

            if (IsOsWindows10TH2OrGreater)
            {
                return OperatingSystemVersion.Windows10TH2;
            }

            if (IsOsWindows10OrGreater)
            {
                return OperatingSystemVersion.Windows10;
            }

            if (IsOsWindows8Point1OrGreater)
            {
                return OperatingSystemVersion.Windows8Point1;
            }

            if (IsOsWindows8OrGreater)
            {
                return OperatingSystemVersion.Windows8;
            }

            if (IsOsWindows7SP1OrGreater)
            {
                return OperatingSystemVersion.Windows7SP1;
            }

            if (IsOsWindows7OrGreater)
            {
                return OperatingSystemVersion.Windows7;
            }

            if (IsOsWindowsVistaSP2OrGreater)
            {
                return OperatingSystemVersion.WindowsVistaSP2;
            }

            if (IsOsWindowsVistaSP1OrGreater)
            {
                return OperatingSystemVersion.WindowsVistaSP1;
            }

            if (IsOsWindowsVistaOrGreater)
            {
                return OperatingSystemVersion.WindowsVista;
            }

            if (IsOsWindowsXPSP3OrGreater)
            {
                return OperatingSystemVersion.WindowsXPSP3;
            }

            if (IsOsWindowsXPSP2OrGreater)
            {
                return OperatingSystemVersion.WindowsXPSP2;
            }

            throw new Exception("OSVersionHelper.GetOsVersion Could not detect OS!");
        }


        public static bool IsVersionOrLater(OperatingSystemVersion version)
        {
            // 
            int major;
            int minor;
            PlatformID platform = PlatformID.Win32NT;
            switch (version)
            {
                case OperatingSystemVersion.Windows8:
                    major = 6;
                    minor = 2;
                    break;

                case OperatingSystemVersion.Windows7:
                    major = 6;
                    minor = 1;
                    break;

                case OperatingSystemVersion.WindowsVista:
                    major = 6;
                    minor = 0;
                    break;

                case OperatingSystemVersion.WindowsXPSP2:
                default:
                    major = 5;
                    minor = 1;
                    break;
            }

            OperatingSystem os = Environment.OSVersion;
            return (os.Platform == platform) &&
                (((os.Version.Major == major) && (os.Version.Minor >= minor)) || (os.Version.Major > major));
        }


    }
}
