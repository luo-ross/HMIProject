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
     Guid(IID.ObjectArray),
 ]
  public  interface IObjectCollection : IObjectArray
    {
        #region IObjectArray redeclarations
        new uint GetCount();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        new object GetAt([In] uint uiIndex, [In] ref Guid riid);
        #endregion

        void AddObject([MarshalAs(UnmanagedType.IUnknown)] object punk);
        void AddFromArray(IObjectArray poaSource);
        void RemoveObjectAt(uint uiIndex);
        void Clear();
    }
}
