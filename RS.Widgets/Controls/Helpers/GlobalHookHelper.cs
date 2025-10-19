using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public static class GlobalHookHelper
    {
        public delegate void GlobalKeyboardEventHandler(object? sender, GlobalKeyboardEventArgs e);
        public delegate void GlobalMouseEventHandler(object? sender, GlobalMouseEventArgs e);

        public static event GlobalKeyboardEventHandler? KeyDown;
        public static event GlobalKeyboardEventHandler? KeyUp;

        public static event GlobalMouseEventHandler? MouseMove;
        public static event GlobalMouseEventHandler? LeftButtonDown;
        public static event GlobalMouseEventHandler? LeftButtonUp;
        public static event GlobalMouseEventHandler? LeftButtonDoubleClick;
        public static event GlobalMouseEventHandler? RightButtonDown;
        public static event GlobalMouseEventHandler? RightButtonUp;
        public static event GlobalMouseEventHandler? RightButtonDoubleClick;
        public static event GlobalMouseEventHandler? MiddleButtonDown;
        public static event GlobalMouseEventHandler? MiddleButtonUp;
        public static event GlobalMouseEventHandler? MiddleButtonDoubleClick;
        public static event GlobalMouseEventHandler? XButtonDown;
        public static event GlobalMouseEventHandler? XButtonUp;
        public static event GlobalMouseEventHandler? XButtonDoubleClick;
        public static event GlobalMouseEventHandler? MouseWheel;


        private static HandleRef KeyboardHookId;
        private static HandleRef MouseHookId;
        private static NativeMethods.HookProc KeyboardHookProc = KeyboardHookCallback;
        private static NativeMethods.HookProc MouseHookProc = MouseHookCallback;

        public static void Start()
        {
            if (KeyboardHookId.Handle == IntPtr.Zero)
            {
                KeyboardHookId = SetHook(KeyboardHookProc, HookType.WH_KEYBOARD_LL);
            }

            if (MouseHookId.Handle == IntPtr.Zero)
            {
                MouseHookId = SetHook(MouseHookProc, HookType.WH_MOUSE_LL);
            }
        }


        public static void Stop()
        {
            if (KeyboardHookId.Handle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(KeyboardHookId);
                KeyboardHookId = default;
            }

            if (MouseHookId.Handle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(MouseHookId);
                MouseHookId = default;
            }
        }

        private static HandleRef SetHook(NativeMethods.HookProc proc, HookType hookType)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(hookType, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                switch ((int)wParam)
                {
                    case NativeMethods.WM_KEYDOWN:
                    case NativeMethods.WM_SYSKEYDOWN:
                        KeyDown?.Invoke(null, new GlobalKeyboardEventArgs(
                            (Keys)kbStruct.vkCode,
                            kbStruct.scanCode,
                            wParam == NativeMethods.WM_SYSKEYDOWN));
                        break;

                    case NativeMethods.WM_KEYUP:
                    case NativeMethods.WM_SYSKEYUP:
                        KeyUp?.Invoke(null, new GlobalKeyboardEventArgs(
                            (Keys)kbStruct.vkCode,
                            kbStruct.scanCode,
                            wParam == NativeMethods.WM_SYSKEYUP));
                        break;
                }
            }
            return NativeMethods.CallNextHookEx(KeyboardHookId, nCode, wParam, lParam);
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

#if !DEBUG

            if (nCode >= 0)
            {
                var mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                TriggerMouseEvents(wParam, mouseStruct);
            }

#endif
            return NativeMethods.CallNextHookEx(MouseHookId, nCode, wParam, lParam);
        }

        private static void TriggerMouseEvents(IntPtr wParam, MSLLHOOKSTRUCT mouseStruct)
        {
            int x = mouseStruct.pt.X;
            int y = mouseStruct.pt.Y;

            switch ((int)wParam)
            {
                case NativeMethods.WM_MOUSEMOVE:
                    MouseMove?.Invoke(null, new GlobalMouseEventArgs(x, y));
                    break;

                case NativeMethods.WM_LBUTTONDOWN:
                    LeftButtonDown?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Left));
                    break;

                case NativeMethods.WM_LBUTTONUP:
                    LeftButtonUp?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Left));
                    break;

                case NativeMethods.WM_LBUTTONDBLCLK:
                    LeftButtonDoubleClick?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Left, 2));
                    break;

                case NativeMethods.WM_RBUTTONDOWN:
                    RightButtonDown?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Right));
                    break;

                case NativeMethods.WM_RBUTTONUP:
                    RightButtonUp?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Right));
                    break;

                case NativeMethods.WM_RBUTTONDBLCLK:
                    RightButtonDoubleClick?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Right, 2));
                    break;

                case NativeMethods.WM_MBUTTONDOWN:
                    MiddleButtonDown?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Middle));
                    break;

                case NativeMethods.WM_MBUTTONUP:
                    MiddleButtonUp?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Middle));
                    break;

                case NativeMethods.WM_MBUTTONDBLCLK:
                    MiddleButtonDoubleClick?.Invoke(null, new GlobalMouseEventArgs(x, y, MouseButton.Middle, 2));
                    break;

                case NativeMethods.WM_XBUTTONDOWN:
                    MouseButton xButton = GetXButton((int)mouseStruct.mouseData);
                    XButtonDown?.Invoke(null, new GlobalMouseEventArgs(x, y, xButton));
                    break;

                case NativeMethods.WM_XBUTTONUP:
                    xButton = GetXButton((int)mouseStruct.mouseData);
                    XButtonUp?.Invoke(null, new GlobalMouseEventArgs(x, y, xButton));
                    break;

                case NativeMethods.WM_XBUTTONDBLCLK:
                    xButton = GetXButton((int)mouseStruct.mouseData);
                    XButtonDoubleClick?.Invoke(null, new GlobalMouseEventArgs(x, y, xButton, 2));
                    break;

                case NativeMethods.WM_MOUSEWHEEL:
                    short delta = (short)((mouseStruct.mouseData >> 16) & 0xFFFF);
                    MouseWheel?.Invoke(null, new GlobalMouseEventArgs(x, y, delta));
                    break;
            }
        }


        private static MouseButton GetXButton(int mouseData)
        {
            int buttonId = (mouseData >> 16) & 0xFFFF;
            return buttonId == NativeMethods.XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
        }

    }
}
