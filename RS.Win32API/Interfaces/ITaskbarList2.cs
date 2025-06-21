using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.TaskbarList2),
    ]
    public interface ITaskbarList2 : ITaskbarList
    {
        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab(IntPtr hwnd);
        new void DeleteTab(IntPtr hwnd);
        new void ActivateTab(IntPtr hwnd);
        new void SetActiveAlt(IntPtr hwnd);
        #endregion

        /// <summary>
        /// Marks a window as full-screen.
        /// </summary>
        /// <param name="hwnd">The handle of the window to be marked.</param>
        /// <param name="fFullscreen">A Boolean value marking the desired full-screen status of the window.</param>
        /// <remarks>
        /// Setting the value of fFullscreen to true, the Shell treats this window as a full-screen window, and the taskbar
        /// is moved to the bottom of the z-order when this window is active.  Setting the value of fFullscreen to false
        /// removes the full-screen marking, but <i>does not</i> cause the Shell to treat the window as though it were
        /// definitely not full-screen.  With a false fFullscreen value, the Shell depends on its automatic detection facility
        /// to specify how the window should be treated, possibly still flagging the window as full-screen.
        /// </remarks>
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
    }

}
