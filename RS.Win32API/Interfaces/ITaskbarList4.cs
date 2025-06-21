using RS.Win32API.Enums;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
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
      Guid(IID.TaskbarList3),
  ]
    public interface ITaskbarList4 : ITaskbarList3
    {
        #region ITaskbarList3 redeclaration

        #region ITaskbarList2 redeclaration

        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab(IntPtr hwnd);
        new void DeleteTab(IntPtr hwnd);
        new void ActivateTab(IntPtr hwnd);
        new void SetActiveAlt(IntPtr hwnd);
        #endregion

        new void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        #endregion

        [PreserveSig] new HRESULT SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        [PreserveSig] new HRESULT SetProgressState(IntPtr hwnd, TBPF tbpFlags);
        [PreserveSig] new HRESULT RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        [PreserveSig] new HRESULT UnregisterTab(IntPtr hwndTab);
        [PreserveSig] new HRESULT SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        [PreserveSig] new HRESULT SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);
        [PreserveSig] new HRESULT ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);
        [PreserveSig] new HRESULT ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);
        [PreserveSig] new HRESULT ThumbBarSetImageList(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);
        [PreserveSig] new HRESULT SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        [PreserveSig] new HRESULT SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        // Using RefRECT to making passing NULL possible.  Removes clipping from the HWND.
        [PreserveSig] new HRESULT SetThumbnailClip(IntPtr hwnd, RefRECT prcClip);

        #endregion

        void SetTabProperties(IntPtr hwndTab, STPF stpFlags);
    }
}
