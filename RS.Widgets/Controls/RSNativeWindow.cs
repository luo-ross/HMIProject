using Microsoft.Win32;
using RS.Win32API.Structs;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using RS.Win32API.Handles;
using System.Windows;

namespace RS.Widgets.Controls
{
    public class RSNativeWindow
    {
        private struct HandleBucket
        {
            public IntPtr handle;

            public GCHandle window;

            public int hash_coll;
        }

        private class WindowClass
        {
            internal static WindowClass cache;

            internal WindowClass next;

            internal string className;

            internal int classStyle;

            internal string windowClassName;

            internal int hashCode;

            internal IntPtr defWindowProc;

            internal NativeMethods.WndProc windowProc;

            internal bool registered;

            internal RSNativeWindow targetWindow;

            private static object wcInternalSyncObject = new object();

            private static int domainQualifier = 0;

            internal WindowClass(string className, int classStyle)
            {
                this.className = className;
                this.classStyle = classStyle;
                RegisterClass();
            }

            public IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
            {
                NativeMethods.SetWindowLong(new HandleRef(null, hWnd), NativeMethods.GWL_WNDPROC, new HandleRef(this, defWindowProc));
                targetWindow.AssignHandle(hWnd);
                return targetWindow.Callback(hWnd, msg, wparam, lparam);
            }

            internal static WindowClass Create(string className, int classStyle)
            {
                lock (wcInternalSyncObject)
                {
                    WindowClass windowClass = cache;
                    if (className == null)
                    {
                        while (windowClass != null && (windowClass.className != null || windowClass.classStyle != classStyle))
                        {
                            windowClass = windowClass.next;
                        }
                    }
                    else
                    {
                        while (windowClass != null && !className.Equals(windowClass.className))
                        {
                            windowClass = windowClass.next;
                        }
                    }

                    if (windowClass == null)
                    {
                        windowClass = new WindowClass(className, classStyle);
                        windowClass.next = cache;
                        cache = windowClass;
                    }
                    else if (!windowClass.registered)
                    {
                        windowClass.RegisterClass();
                    }

                    return windowClass;
                }
            }

            internal static void DisposeCache()
            {
                lock (wcInternalSyncObject)
                {
                    for (WindowClass windowClass = cache; windowClass != null; windowClass = windowClass.next)
                    {
                        windowClass.UnregisterClass();
                    }
                }
            }

            private string GetFullClassName(string className)
            {
                StringBuilder stringBuilder = new StringBuilder(50);
                stringBuilder.Append(RuntimeInformation.FrameworkDescription);
                stringBuilder.Append('.');
                stringBuilder.Append(className);
                stringBuilder.Append(".app.");
                stringBuilder.Append(domainQualifier);
                stringBuilder.Append('.');
                string name = Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 16);
                stringBuilder.Append(VersioningHelper.MakeVersionSafeName(name, ResourceScope.Process, ResourceScope.AppDomain));
                return stringBuilder.ToString();
            }

            private void RegisterClass()
            {
                WNDCLASS_D wNDCLASS_D = new WNDCLASS_D();
                if (userDefWindowProc == IntPtr.Zero)
                {
                    string lpProcName = ((Marshal.SystemDefaultCharSize == 1) ? "DefWindowProcA" : "DefWindowProcW");
                    userDefWindowProc = NativeMethods.GetProcAddress(new HandleRef(null, NativeMethods.GetModuleHandle("user32.dll")), lpProcName);
                    if (userDefWindowProc == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }

                string text = className;
                if (text == null)
                {
                    wNDCLASS_D.hbrBackground = NativeMethods.GetStockObject(5);
                    wNDCLASS_D.style = classStyle;
                    defWindowProc = userDefWindowProc;
                    text = "Window." + Convert.ToString(classStyle, 16);
                    hashCode = 0;
                }
                else
                {
                    WNDCLASS_I wNDCLASS_I = new WNDCLASS_I();
                    bool classInfo = NativeMethods.GetClassInfo(NativeMethods.NullHandleRef, className, wNDCLASS_I);
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (!classInfo)
                    {
                        throw new Win32Exception(lastWin32Error, "InvalidWndClsName");
                    }

                    wNDCLASS_D.style = wNDCLASS_I.style;
                    wNDCLASS_D.cbClsExtra = wNDCLASS_I.cbClsExtra;
                    wNDCLASS_D.cbWndExtra = wNDCLASS_I.cbWndExtra;
                    wNDCLASS_D.hIcon = wNDCLASS_I.hIcon;
                    wNDCLASS_D.hCursor = wNDCLASS_I.hCursor;
                    wNDCLASS_D.hbrBackground = wNDCLASS_I.hbrBackground;
                    wNDCLASS_D.lpszMenuName = Marshal.PtrToStringAuto(wNDCLASS_I.lpszMenuName);
                    text = className;
                    defWindowProc = wNDCLASS_I.lpfnWndProc;
                    hashCode = className.GetHashCode();
                }

                windowClassName = GetFullClassName(text);
                windowProc = Callback;
                wNDCLASS_D.lpfnWndProc = windowProc;
                wNDCLASS_D.hInstance = NativeMethods.GetModuleHandle(null);
                wNDCLASS_D.lpszClassName = windowClassName;
                short num = NativeMethods.RegisterClass(wNDCLASS_D);
                if (num == 0)
                {
                    int lastWin32Error2 = Marshal.GetLastWin32Error();
                    if (lastWin32Error2 == 1410)
                    {
                        WNDCLASS_I wNDCLASS_I2 = new WNDCLASS_I();
                        if (NativeMethods.GetClassInfo(new HandleRef(null, NativeMethods.GetModuleHandle(null)), windowClassName, wNDCLASS_I2) && wNDCLASS_I2.lpfnWndProc == UserDefindowProc)
                        {
                            if (NativeMethods.UnregisterClass(windowClassName, new HandleRef(null, NativeMethods.GetModuleHandle(null))))
                            {
                                num = NativeMethods.RegisterClass(wNDCLASS_D);
                            }
                            else
                            {
                                do
                                {
                                    domainQualifier++;
                                    windowClassName = GetFullClassName(text);
                                    wNDCLASS_D.lpszClassName = windowClassName;
                                    num = NativeMethods.RegisterClass(wNDCLASS_D);
                                }
                                while (num == 0 && Marshal.GetLastWin32Error() == 1410);
                            }
                        }
                    }

                    if (num == 0)
                    {
                        windowProc = null;
                        throw new Win32Exception(lastWin32Error2);
                    }
                }

                registered = true;
            }

            private void UnregisterClass()
            {
                if (registered && NativeMethods.UnregisterClass(windowClassName, new HandleRef(null, NativeMethods.GetModuleHandle(null))))
                {
                    windowProc = null;
                    registered = false;
                }
            }
        }



        [ThreadStatic]
        private static byte wndProcFlags;
        [ThreadStatic]
        private static byte userSetProcFlags;
        [ThreadStatic]
        private static bool anyHandleCreated;
        private static readonly int[] primes;
        private static short globalID;
        private static object internalSyncObject;
        private static object createWindowSyncObject;
        private static int hashLoadSize;
        private static HandleBucket[] hashBuckets;
        private static IntPtr userDefWindowProc;
        private static int handleCount;
        private static Dictionary<short, IntPtr> hashForIdHandle;
        private static Dictionary<IntPtr, short> hashForHandleId;
        private static byte userSetProcFlagsForApp;
        private static bool anyHandleCreatedInApp;
        internal static IntPtr UserDefindowProc => userDefWindowProc;
        private static int WndProcFlags
        {
            get
            {
                int num = wndProcFlags;
                if (num == 0)
                {
                    if (userSetProcFlags != 0)
                    {
                        num = userSetProcFlags;
                    }
                    else if (userSetProcFlagsForApp != 0)
                    {
                        num = userSetProcFlagsForApp;
                    }
                    num |= 1;
                    wndProcFlags = (byte)num;
                }
                return num;
            }
        }

        internal static bool WndProcShouldBeDebuggable => (WndProcFlags & 4) != 0;

        private GCHandle rootRef;
        private HwndSourceParameters HwndSourceParameters;
        private HwndSource HwndSource;
        private WeakReference weakThisPtr;
        private RSNativeWindow previousWindow;
        private IntPtr defWindowProc;
        private IntPtr windowProcPtr;
        private NativeMethods.WndProc windowProc;
        private RSNativeWindow nextWindow;
        private bool ownHandle;
        private IntPtr handle;
        private RSNativeWindow targetWindow;
        private bool suppressedGC;

        //
        // 摘要:
        //     Gets the handle for this window.
        //
        // 返回结果:
        //     If successful, an System.IntPtr representing the handle to the associated native
        //     Win32 window; otherwise, 0 if no handle is associated with the window.
        public IntPtr Handle => handle;
        internal RSNotifyIcon reference;
        internal RSNativeWindow PreviousWindow => previousWindow;
        static RSNativeWindow()
        {
            primes = new int[70]
            {
            11, 17, 23, 29, 37, 47, 59, 71, 89, 107,
            131, 163, 197, 239, 293, 353, 431, 521, 631, 761,
            919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861,
            5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293,
            36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751,
            225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687,
            1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
            };
            wndProcFlags = 0;
            userSetProcFlags = 0;
            globalID = 1;
            internalSyncObject = new object();
            createWindowSyncObject = new object();
            EventHandler value = OnShutdown;
            AppDomain.CurrentDomain.ProcessExit += value;
            AppDomain.CurrentDomain.DomainUnload += value;
            int num = primes[4];
            hashBuckets = new HandleBucket[num];
            hashLoadSize = (int)(0.72f * (float)num);
            if (hashLoadSize >= num)
            {
                hashLoadSize = num - 1;
            }
            hashForIdHandle = new Dictionary<short, IntPtr>();
            hashForHandleId = new Dictionary<IntPtr, short>();
        }

        internal RSNativeWindow()
        {
            weakThisPtr = new WeakReference(this);
        }

        //
        // 摘要:
        //     Releases the resources associated with this window.
        ~RSNativeWindow()
        {
            ForceExitMessageLoop();
        }





        internal void ForceExitMessageLoop()
        {
            IntPtr intPtr;
            bool flag;
            lock (this)
            {
                intPtr = handle;
                flag = ownHandle;
            }

            if (handle != IntPtr.Zero)
            {
                if (NativeMethods.IsWindow(new HandleRef(null, handle)))
                {
                    int lpdwProcessId;
                    int windowThreadProcessId = NativeMethods.GetWindowThreadProcessId(new HandleRef(null, handle), out lpdwProcessId);
                    //IntPtr intPtr2 = Application.ThreadContext.FromId(windowThreadProcessId)?.GetHandle() ?? IntPtr.Zero;
                    //if (intPtr2 != IntPtr.Zero)
                    //{
                    //    int lpdwExitCode = 0;
                    //    NativeMethods.GetExitCodeThread(new HandleRef(null, intPtr2), out lpdwExitCode);
                    //    if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && lpdwExitCode == 259)
                    //    {
                    //        _ = NativeMethods.SendMessageTimeout(new HandleRef(null, handle), NativeMethods.WM_UIUNSUBCLASS, IntPtr.Zero, IntPtr.Zero, 2, 100, out var _) == IntPtr.Zero;
                    //    }
                    //}
                }

                if (handle != IntPtr.Zero)
                {
                    ReleaseHandle(handleValid: true);
                }
            }

            if (intPtr != IntPtr.Zero && flag)
            {
                NativeMethods.PostMessage(new HandleRef(this, intPtr), NativeMethods.WM_CLOSE, 0, 0);
            }
        }


        // 摘要:
        //     Releases the handle associated with this window.
        public virtual void ReleaseHandle()
        {
            ReleaseHandle(handleValid: true);
        }


        private void ReleaseHandle(bool handleValid)
        {
            if (!(handle != IntPtr.Zero))
            {
                return;
            }

            lock (this)
            {
                if (!(handle != IntPtr.Zero))
                {
                    return;
                }

                if (handleValid)
                {
                    UnSubclass();
                }

                RemoveWindowFromTable(handle, this);

                if (ownHandle)
                {
                    Win32API.Handles.HandleCollector.Remove(handle, CommonHandles.Window);
                    ownHandle = false;
                }

                handle = IntPtr.Zero;
                if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
                {
                    OnHandleChange();
                    GC.SuppressFinalize(this);
                    suppressedGC = true;
                }
            }
        }

        //
        // 摘要:
        //     Specifies a notification method that is called when the handle for a window is
        //     changed.
        protected virtual void OnHandleChange()
        {

        }


        internal static void RemoveWindowFromIDTable(IntPtr handle)
        {
            short key = hashForHandleId[handle];
            hashForHandleId.Remove(handle);
            hashForIdHandle.Remove(key);
        }

        internal static void SetUnhandledExceptionModeInternal(UnhandledExceptionMode mode, bool threadScope)
        {
            if (!threadScope && anyHandleCreatedInApp)
            {
                throw new InvalidOperationException("ApplicationCannotChangeApplicationExceptionMode");
            }

            if (threadScope && anyHandleCreated)
            {
                throw new InvalidOperationException("ApplicationCannotChangeThreadExceptionMode");
            }

            switch (mode)
            {
                case UnhandledExceptionMode.Automatic:
                    if (threadScope)
                    {
                        userSetProcFlags = 0;
                    }
                    else
                    {
                        userSetProcFlagsForApp = 0;
                    }

                    break;
                case UnhandledExceptionMode.ThrowException:
                    if (threadScope)
                    {
                        userSetProcFlags = 5;
                    }
                    else
                    {
                        userSetProcFlagsForApp = 5;
                    }

                    break;
                case UnhandledExceptionMode.CatchException:
                    if (threadScope)
                    {
                        userSetProcFlags = 1;
                    }
                    else
                    {
                        userSetProcFlagsForApp = 1;
                    }

                    break;
                default:
                    throw new InvalidEnumArgumentException("mode", (int)mode, typeof(UnhandledExceptionMode));
            }
        }


        //public virtual void CreateHandle()
        //{
        //    var windowName = @$"{nameof(RSNotifyIcon)}{Guid.NewGuid()}";
        //    this.HwndSourceParameters = new HwndSourceParameters(windowName);
        //    this.HwndSourceParameters.Width = 0;
        //    this.HwndSourceParameters.Height = 0;
        //    this.HwndSourceParameters.WindowStyle = 0;
        //    this.HwndSourceParameters.TreatAsInputRoot = true;
        //    this.HwndSource = new HwndSource(this.HwndSourceParameters);
        //    windowProc = Callback;
        //    this.HwndSource.AddHook(new HwndSourceHook(HwndSourceHook));
        //    ownHandle = true;
        //    Win32API.Handles.HandleCollector.Add(this.HwndSource.Handle, CommonHandles.Window);
        //}

        public virtual void CreateHandle(CreateParams cp)
        {
            lock (this)
            {
                CheckReleased();
                WindowClass windowClass = WindowClass.Create(cp.ClassName, cp.ClassStyle);
                lock (createWindowSyncObject)
                {
                    if (handle != IntPtr.Zero)
                    {
                        return;
                    }

                    windowClass.targetWindow = this;
                    IntPtr intPtr = IntPtr.Zero;
                    int error = 0;

                    IntPtr moduleHandle = NativeMethods.GetModuleHandle(null);
                    try
                    {
                        if (cp.Caption != null && cp.Caption.Length > 32767)
                        {
                            cp.Caption = cp.Caption.Substring(0, 32767);
                        }

                        intPtr = NativeMethods.CreateWindowEx(cp.ExStyle, windowClass.windowClassName, cp.Caption, cp.Style, cp.X, cp.Y, cp.Width, cp.Height, new HandleRef(cp, cp.Parent), NativeMethods.NullHandleRef, new HandleRef(null, moduleHandle), cp.Param);
                        error = Marshal.GetLastWin32Error();
                    }
                    catch (NullReferenceException innerException)
                    {
                        throw new OutOfMemoryException("ErrorCreatingHandle", innerException);
                    }

                    windowClass.targetWindow = null;
                    if (intPtr == IntPtr.Zero)
                    {
                        throw new Win32Exception(error, "ErrorCreatingHandle");
                    }

                    ownHandle = true;
                    Win32API.Handles.HandleCollector.Add(intPtr, CommonHandles.Window);
                }
            }
        }

        //
        // 摘要:
        //     Destroys the window and its handle.
        public virtual void DestroyHandle()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    if (!NativeMethods.DestroyWindow(new HandleRef(this, handle)))
                    {
                        UnSubclass();
                        NativeMethods.PostMessage(new HandleRef(this, handle), 16, 0, 0);
                    }

                    handle = IntPtr.Zero;
                    ownHandle = false;
                }
                GC.SuppressFinalize(this);
                suppressedGC = true;
            }
        }


        public static RSNativeWindow FromHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero && handleCount > 0)
            {
                return GetWindowFromTable(handle);
            }

            return null;
        }

        private static RSNativeWindow GetWindowFromTable(IntPtr handle)
        {
            HandleBucket[] array = hashBuckets;
            int num = 0;
            uint seed;
            uint incr;
            uint num2 = InitHash(handle, array.Length, out seed, out incr);
            HandleBucket handleBucket;
            do
            {
                int num3 = (int)(seed % (uint)array.Length);
                handleBucket = array[num3];
                if (handleBucket.handle == IntPtr.Zero)
                {
                    return null;
                }

                if ((handleBucket.hash_coll & 0x7FFFFFFF) == num2 && handle == handleBucket.handle && handleBucket.window.IsAllocated)
                {
                    return (RSNativeWindow)handleBucket.window.Target;
                }

                seed += incr;
            }
            while (handleBucket.hash_coll < 0 && ++num < array.Length);
            return null;
        }

        internal IntPtr GetHandleFromID(short id)
        {
            if (hashForIdHandle == null || !hashForIdHandle.TryGetValue(id, out var value))
            {
                return IntPtr.Zero;
            }

            return value;
        }



        public IntPtr HwndSourceHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            NativeMethods.SetWindowLong(new HandleRef(null, hwnd), NativeMethods.GWL_WNDPROC, new HandleRef(this, defWindowProc));
            targetWindow.AssignHandle(hwnd);
            return targetWindow.Callback(hwnd, msg, wParam, lParam);
        }


        [PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            if (handleCount > 0)
            {
                lock (internalSyncObject)
                {
                    for (int i = 0; i < hashBuckets.Length; i++)
                    {
                        HandleBucket handleBucket = hashBuckets[i];
                        if (handleBucket.handle != IntPtr.Zero && handleBucket.handle != new IntPtr(-1))
                        {
                            HandleRef handleRef = new HandleRef(handleBucket, handleBucket.handle);
                            NativeMethods.SetWindowLong(handleRef, NativeMethods.GWL_WNDPROC, new HandleRef(null, userDefWindowProc));
                            NativeMethods.SetClassLong(handleRef, NativeMethods.GCL_WNDPROC, userDefWindowProc);
                            NativeMethods.PostMessage(handleRef, NativeMethods.WM_CLOSE, 0, 0);
                            if (handleBucket.window.IsAllocated)
                            {
                                RSNativeWindow nativeWindow = (RSNativeWindow)handleBucket.window.Target;
                                if (nativeWindow != null)
                                {
                                    nativeWindow.handle = IntPtr.Zero;
                                }
                            }

                            handleBucket.window.Free();
                        }

                        hashBuckets[i].handle = IntPtr.Zero;
                        hashBuckets[i].hash_coll = 0;
                    }

                    handleCount = 0;
                }
            }

            WindowClass.DisposeCache();
        }

        public void DefWndProc(ref Message m)
        {
            if (previousWindow == null)
            {
                if (defWindowProc == IntPtr.Zero)
                {
                    m.Result = NativeMethods.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
                }
                else
                {
                    m.Result = NativeMethods.CallWindowProc(defWindowProc, m.HWnd, m.Msg, m.WParam, m.LParam);
                }
            }
            else
            {
                m.Result = previousWindow.Callback(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
        }

        private IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            Message m = Message.Create(hWnd, msg, wparam, lparam);
            try
            {
                if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
                {
                    WndProc(ref m);
                }
                else
                {
                    DefWndProc(ref m);
                }
            }
            catch (Exception e)
            {
                OnThreadException(e);
            }
            finally
            {
                if (msg == NativeMethods.WM_NCDESTROY)
                {
                    ReleaseHandle(handleValid: false);
                }

                if (msg == NativeMethods.WM_UIUNSUBCLASS)
                {
                    ReleaseHandle(handleValid: true);
                }
            }

            return m.Result;
        }



        private static void RemoveWindowFromTable(IntPtr handle, RSNativeWindow window)
        {
            lock (internalSyncObject)
            {
                uint seed;
                uint incr;
                uint num = InitHash(handle, hashBuckets.Length, out seed, out incr);
                int num2 = 0;
                RSNativeWindow value = window.PreviousWindow;
                int num3;
                do
                {
                    num3 = (int)(seed % (uint)hashBuckets.Length);
                    HandleBucket handleBucket = hashBuckets[num3];
                    if ((handleBucket.hash_coll & 0x7FFFFFFF) == num && handle == handleBucket.handle)
                    {
                        bool flag = window.nextWindow == null;
                        bool flag2 = IsRootWindowInListWithChildren(window);
                        if (window.previousWindow != null)
                        {
                            window.previousWindow.nextWindow = window.nextWindow;
                        }

                        if (window.nextWindow != null)
                        {
                            window.nextWindow.defWindowProc = window.defWindowProc;
                            window.nextWindow.previousWindow = window.previousWindow;
                        }

                        window.nextWindow = null;
                        window.previousWindow = null;
                        if (flag2)
                        {
                            if (hashBuckets[num3].window.IsAllocated)
                            {
                                hashBuckets[num3].window.Free();
                            }

                            hashBuckets[num3].window = GCHandle.Alloc(value, GCHandleType.Weak);
                        }
                        else if (flag)
                        {
                            hashBuckets[num3].hash_coll &= int.MinValue;
                            if (hashBuckets[num3].hash_coll != 0)
                            {
                                hashBuckets[num3].handle = new IntPtr(-1);
                            }
                            else
                            {
                                hashBuckets[num3].handle = IntPtr.Zero;
                            }

                            if (hashBuckets[num3].window.IsAllocated)
                            {
                                hashBuckets[num3].window.Free();
                            }

                            handleCount--;
                        }

                        break;
                    }

                    seed += incr;
                }
                while (hashBuckets[num3].hash_coll < 0 && ++num2 < hashBuckets.Length);
            }
        }

        private static bool IsRootWindowInListWithChildren(RSNativeWindow window)
        {
            if (window.PreviousWindow != null)
            {
                return window.nextWindow == null;
            }

            return false;
        }

        private static uint InitHash(IntPtr handle, int hashsize, out uint seed, out uint incr)
        {
            uint result = (seed = (uint)handle.GetHashCode() & 0x7FFFFFFFu);
            incr = 1 + ((seed >> 5) + 1) % (uint)(hashsize - 1);
            return result;
        }


        private void UnSubclass()
        {
            bool flag = !weakThisPtr.IsAlive || weakThisPtr.Target == null;
            HandleRef hWnd = new HandleRef(this, this.Handle);
            IntPtr windowLong = NativeMethods.GetWindowLong(new HandleRef(this, this.Handle), NativeMethods.GWL_WNDPROC);
            if (windowProcPtr == windowLong)
            {
                if (previousWindow == null)
                {
                    NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_WNDPROC, new HandleRef(this, defWindowProc));
                }
                else if (flag)
                {
                    NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_WNDPROC, new HandleRef(this, userDefWindowProc));
                }
                else
                {
                    NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_WNDPROC, previousWindow.windowProc);
                }
            }
            else if (nextWindow == null || nextWindow.defWindowProc != windowProcPtr)
            {
                NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_WNDPROC, new HandleRef(this, userDefWindowProc));
            }
        }

        public void AssignHandle(IntPtr handle)
        {
            AssignHandle(handle, assignUniqueID: true);
        }

        internal void AssignHandle(IntPtr handle, bool assignUniqueID)
        {
            lock (this)
            {
                CheckReleased();
                this.handle = handle;


                if (userDefWindowProc == IntPtr.Zero)
                {
                    string lpProcName = ((Marshal.SystemDefaultCharSize == 1) ? "DefWindowProcA" : "DefWindowProcW");
                    userDefWindowProc = NativeMethods.GetProcAddress(new HandleRef(null, NativeMethods.GetModuleHandle("user32.dll")), lpProcName);
                    if (userDefWindowProc == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }

                defWindowProc = NativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC);
                if (WndProcShouldBeDebuggable)
                {
                    windowProc = DebuggableCallback;
                }
                else
                {
                    windowProc = Callback;
                }

                AddWindowToTable(handle, this);
                NativeMethods.SetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC, windowProc);
                windowProcPtr = NativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC);
                if (assignUniqueID && ((uint)(int)(long)NativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_STYLE) & 0x40000000u) != 0 && (int)(long)NativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_ID) == 0)
                {
                    NativeMethods.SetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_ID, new HandleRef(this, handle));
                }

                if (suppressedGC)
                {
                    GC.ReRegisterForFinalize(this);
                    suppressedGC = false;
                }

                OnHandleChange();
            }
        }

        private static void AddWindowToTable(IntPtr handle, RSNativeWindow window)
        {
            lock (internalSyncObject)
            {
                if (handleCount >= hashLoadSize)
                {
                    ExpandTable();
                }

                anyHandleCreated = true;
                anyHandleCreatedInApp = true;
                uint seed;
                uint incr;
                uint num = InitHash(handle, hashBuckets.Length, out seed, out incr);
                int num2 = 0;
                int num3 = -1;
                GCHandle window2 = GCHandle.Alloc(window, GCHandleType.Weak);
                do
                {
                    int num4 = (int)(seed % (uint)hashBuckets.Length);
                    if (num3 == -1 && hashBuckets[num4].handle == new IntPtr(-1) && hashBuckets[num4].hash_coll < 0)
                    {
                        num3 = num4;
                    }

                    if (hashBuckets[num4].handle == IntPtr.Zero || (hashBuckets[num4].handle == new IntPtr(-1) && (hashBuckets[num4].hash_coll & 0x80000000u) == 0L))
                    {
                        if (num3 != -1)
                        {
                            num4 = num3;
                        }

                        hashBuckets[num4].window = window2;
                        hashBuckets[num4].handle = handle;
                        hashBuckets[num4].hash_coll |= (int)num;
                        handleCount++;
                        return;
                    }

                    if ((hashBuckets[num4].hash_coll & 0x7FFFFFFF) == num && handle == hashBuckets[num4].handle)
                    {
                        GCHandle window3 = hashBuckets[num4].window;
                        if (window3.IsAllocated)
                        {
                            if (window3.Target != null)
                            {
                                window.previousWindow = (RSNativeWindow)window3.Target;
                                window.previousWindow.nextWindow = window;
                            }

                            window3.Free();
                        }

                        hashBuckets[num4].window = window2;
                        return;
                    }

                    if (num3 == -1)
                    {
                        hashBuckets[num4].hash_coll |= int.MinValue;
                    }

                    seed += incr;
                }
                while (++num2 < hashBuckets.Length);
                if (num3 != -1)
                {
                    hashBuckets[num3].window = window2;
                    hashBuckets[num3].handle = handle;
                    hashBuckets[num3].hash_coll |= (int)num;
                    handleCount++;
                }
            }
        }

        private static void ExpandTable()
        {
            int num = hashBuckets.Length;
            int prime = GetPrime(1 + num * 2);
            HandleBucket[] array = new HandleBucket[prime];
            for (int i = 0; i < num; i++)
            {
                HandleBucket handleBucket = hashBuckets[i];
                if (!(handleBucket.handle != IntPtr.Zero) || !(handleBucket.handle != new IntPtr(-1)))
                {
                    continue;
                }

                uint num2 = (uint)handleBucket.hash_coll & 0x7FFFFFFFu;
                uint num3 = 1 + ((num2 >> 5) + 1) % (uint)(array.Length - 1);
                int num4;
                while (true)
                {
                    num4 = (int)(num2 % (uint)array.Length);
                    if (array[num4].handle == IntPtr.Zero || array[num4].handle == new IntPtr(-1))
                    {
                        break;
                    }

                    array[num4].hash_coll |= int.MinValue;
                    num2 += num3;
                }

                array[num4].window = handleBucket.window;
                array[num4].handle = handleBucket.handle;
                array[num4].hash_coll |= handleBucket.hash_coll & 0x7FFFFFFF;
            }

            hashBuckets = array;
            hashLoadSize = (int)(0.72f * (float)prime);
            if (hashLoadSize >= prime)
            {
                hashLoadSize = prime - 1;
            }
        }

        private static int GetPrime(int minSize)
        {
            if (minSize < 0)
            {
                throw new OutOfMemoryException();
            }

            for (int i = 0; i < primes.Length; i++)
            {
                int num = primes[i];
                if (num >= minSize)
                {
                    return num;
                }
            }

            for (int j = (minSize - 2) | 1; j < int.MaxValue; j += 2)
            {
                bool flag = true;
                if (((uint)j & (true ? 1u : 0u)) != 0)
                {
                    int num2 = (int)Math.Sqrt(j);
                    for (int k = 3; k < num2; k += 2)
                    {
                        if (j % k == 0)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        return j;
                    }
                }
                else if (j == 2)
                {
                    return j;
                }
            }

            return minSize;
        }



        private IntPtr DebuggableCallback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            Message m = Message.Create(hWnd, msg, wparam, lparam);
            try
            {
                if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
                {
                    WndProc(ref m);
                }
                else
                {
                    DefWndProc(ref m);
                }
            }
            finally
            {
                if (msg == 130)
                {
                    ReleaseHandle(handleValid: false);
                }

                if (msg == NativeMethods.WM_UIUNSUBCLASS)
                {
                    ReleaseHandle(handleValid: true);
                }
            }

            return m.Result;
        }


        private void CheckReleased()
        {
            if (handle != IntPtr.Zero)
            {
                throw new InvalidOperationException("HandleAlreadyExists");
            }
        }

        protected virtual void OnThreadException(Exception e)
        {
        }

        protected virtual void WndProc(ref Message m)
        {
            DefWndProc(ref m);
        }

    }
}
