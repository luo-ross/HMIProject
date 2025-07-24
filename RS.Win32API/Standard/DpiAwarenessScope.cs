using RS.Win32API.Enums;
using RS.Win32API.Helper;
using RS.Win32API.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Standard
{
    public class DpiAwarenessScope : IDisposable
    {
        public static bool OperationSupported { get; set; } = true;


        public bool IsThreadInMixedHostingBehavior
        {
            [SecuritySafeCritical]
            get
            {
                return NativeMethods.GetThreadDpiHostingBehavior() == DPI_HOSTING_BEHAVIOR.DPI_HOSTING_BEHAVIOR_MIXED;
            }
        }

        public DpiAwarenessContextHandle OldDpiAwarenessContext { get; set; }

        public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue)
            : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode: false, updateIfWindowIsSystemAwareOrUnaware: false, nint.Zero)
        {
        }

        public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode)
            : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: false, nint.Zero)
        {
        }

        public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode, nint hWnd)
            : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: true, hWnd)
        {
        }

        public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextValue, bool updateIfThreadInMixedHostingMode, bool updateIfWindowIsSystemAwareOrUnaware, nint hWnd)
        {
            if (dpiAwarenessContextValue == DpiAwarenessContextValue.Invalid || !OperationSupported || updateIfThreadInMixedHostingMode && !IsThreadInMixedHostingBehavior || updateIfWindowIsSystemAwareOrUnaware && (hWnd == nint.Zero || !IsWindowUnawareOrSystemAware(hWnd)))
            {
                return;
            }

            try
            {
                OldDpiAwarenessContext = NativeMethods.SetThreadDpiAwarenessContext(new DpiAwarenessContextHandle(dpiAwarenessContextValue));
            }
            catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
            {
                OperationSupported = false;
            }
        }

        public void Dispose()
        {
            if (OldDpiAwarenessContext != null)
            {
                NativeMethods.SetThreadDpiAwarenessContext(OldDpiAwarenessContext);
                OldDpiAwarenessContext = null;
            }
        }

        public bool IsWindowUnawareOrSystemAware(nint hWnd)
        {
            DpiAwarenessContextHandle dpiAwarenessContext = SystemDpiHelper.GetDpiAwarenessContext(hWnd);
            if (!dpiAwarenessContext.Equals(DpiAwarenessContextValue.Unaware))
            {
                return dpiAwarenessContext.Equals(DpiAwarenessContextValue.SystemAware);
            }

            return true;
        }
    }
}
