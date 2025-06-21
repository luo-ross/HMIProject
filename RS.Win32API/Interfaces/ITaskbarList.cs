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
        Guid(IID.TaskbarList),
    ]
    public interface ITaskbarList
    {
        /// <summary>
        /// This function must be called first to validate use of other members.
        /// </summary>
        void HrInit();

        /// <summary>
        /// This function adds a tab for hwnd to the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which to add the tab.</param>
        void AddTab(IntPtr hwnd);

        /// <summary>
        /// This function deletes a tab for hwnd from the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be deleted.</param>
        void DeleteTab(IntPtr hwnd);

        /// <summary>
        /// This function activates the tab associated with hwnd on the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be actuvated.</param>
        void ActivateTab(IntPtr hwnd);

        /// <summary>
        /// This function marks hwnd in the taskbar as the active tab.
        /// </summary>
        /// <param name="hwnd">The HWND to activate.</param>
        void SetActiveAlt(IntPtr hwnd);
    }
}
