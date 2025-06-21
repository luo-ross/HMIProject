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
      Guid(IID.EnumObjects),
  ]
    public interface IEnumObjects
    {
        //[local]
        // This signature might not work... Hopefully don't need this interface though.
        void Next(uint celt, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, IidParameterIndex = 1, SizeParamIndex = 0)] object[] rgelt, [Out] out uint pceltFetched);

        /*
        [call_as(Next)] HRESULT RemoteNext(
            [in] ULONG celt,
            [in] REFIID riid,
            [out, size_is(celt), length_is(*pceltFetched), iid_is(riid)] void **rgelt,
            [out] ULONG *pceltFetched);
         */

        void Skip(uint celt);

        void Reset();

        IEnumObjects Clone();
    }
}
