using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RS.Win32API
{
    public static class WpfLibraryLoader
    {
        public const string COMPLUS_Version = "COMPLUS_Version";

        public const string COMPLUS_InstallRoot = "COMPLUS_InstallRoot";

        public const string EnvironmentVariables = "COMPLUS_Version;COMPLUS_InstallRoot";

        public const string FRAMEWORK_RegKey = "Software\\Microsoft\\Net Framework Setup\\NDP\\v4\\Client\\";

        public const string FRAMEWORK_RegKey_FullPath = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Net Framework Setup\\NDP\\v4\\Client\\";

        public const string FRAMEWORK_InstallPath_RegValue = "InstallPath";

        public const string DOTNET_RegKey = "Software\\Microsoft\\.NETFramework";

        public const string DOTNET_Install_RegValue = "InstallRoot";

        public const string WPF_SUBDIR = "WPF";

        public static string WpfInstallPath { get; } = GetWPFInstallPath();




        public static void EnsureLoaded(string dllName)
        {
            NativeMethods.LoadLibrary(Path.Combine(WpfInstallPath, dllName));
        }

        public static string GetWPFInstallPath()
        {
            string text = null;
            string environmentVariable = Environment.GetEnvironmentVariable("COMPLUS_Version");
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                text = Environment.GetEnvironmentVariable("COMPLUS_InstallRoot");
                if (string.IsNullOrEmpty(text))
                {
                    text = ReadLocalMachineString("Software\\Microsoft\\.NETFramework", "InstallRoot");
                }

                if (!string.IsNullOrEmpty(text))
                {
                    text = Path.Combine(text, environmentVariable);
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    text = RuntimeEnvironment.GetRuntimeDirectory();
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = ReadLocalMachineString("Software\\Microsoft\\Net Framework Setup\\NDP\\v4\\Client\\", "InstallPath");
                }
            }

            return Path.Combine(text, "WPF");
        }

        public static string ReadLocalMachineString(string key, string valueName)
        {
            string text = "HKEY_LOCAL_MACHINE\\" + key;
            return Registry.GetValue(text, valueName, null) as string;
        }
    }
}
