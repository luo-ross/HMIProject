using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API
{
    [SuppressUnmanagedCodeSecurity]
    public static class NativeMethodsSetLastError
    {
        static NativeMethodsSetLastError()
        {
            WpfLibraryLoader.EnsureLoaded(ExternDll.PresentationNativeDll);
        }

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "EnableWindowWrapper", ExactSpelling = true, SetLastError = true)]
        public static extern bool EnableWindow(HandleRef hWnd, bool enable);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetAncestorWrapper")]
        public static extern IntPtr GetAncestor(IntPtr hwnd, int gaFlags);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetKeyboardLayoutListWrapper", ExactSpelling = true, SetLastError = true)]
        public static extern int GetKeyboardLayoutList(int size, [Out][MarshalAs(UnmanagedType.LPArray)] IntPtr[] hkls);

        [DllImport(ExternDll.PresentationNativeDll, EntryPoint = "GetParentWrapper", SetLastError = true)]
        public static extern IntPtr GetParent(HandleRef hWnd);

        [DllImport(ExternDll.PresentationNativeDll, EntryPoint = "GetWindowWrapper", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
        public static extern int GetWindowLong(HandleRef hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
        public static extern NativeMethods.WndProc GetWindowLongWndProc(HandleRef hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
        public static extern IntPtr GetWindowLongPtr(HandleRef hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
        public static extern NativeMethods.WndProc GetWindowLongPtrWndProc(HandleRef hWnd, int nIndex);

        [DllImport(ExternDll.PresentationNativeDll, BestFitMapping = false, CharSet = CharSet.Auto, EntryPoint = "GetWindowTextWrapper", SetLastError = true)]
        public static extern int GetWindowText(HandleRef hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "GetWindowTextLengthWrapper", SetLastError = true)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "MapWindowPointsWrapper", ExactSpelling = true, SetLastError = true)]
        public static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, [In][Out] ref RECT rect, int cPoints);

        [DllImport(ExternDll.PresentationNativeDll, EntryPoint = "SetFocusWrapper", SetLastError = true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper")]
        public static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper", SetLastError = true)]
        public static extern int SetWindowLongWndProc(HandleRef hWnd, int nIndex, NativeMethods.WndProc dwNewLong);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper")]
        public static extern IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport(ExternDll.PresentationNativeDll, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtrWndProc(HandleRef hWnd, int nIndex, NativeMethods.WndProc dwNewLong);
    }
}
