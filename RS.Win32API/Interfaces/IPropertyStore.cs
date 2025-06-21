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
        Guid(IID.PropertyStore)
    ]
    public interface IPropertyStore
    {
        uint GetCount();
        PKEY GetAt(uint iProp);

        /// <SecurityNote>
        ///   Critical : Accepts critical PROPVARIANT argument
        /// <SecurityNote>

        void GetValue([In] ref PKEY pkey, [In, Out] PROPVARIANT pv);

        /// <SecurityNote>
        ///   Critical : Accepts critical PROPVARIANT argument
        /// <SecurityNote>

        void SetValue([In] ref PKEY pkey, PROPVARIANT pv);

        void Commit();
    }
}
