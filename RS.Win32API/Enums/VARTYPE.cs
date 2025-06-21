using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    public enum VARTYPE : short
    {
        /// <summary>
        /// BSTR
        /// </summary>
        VT_BSTR = 8,        // BSTR allocated using SysAllocString

        /// <summary>
        /// LPSTR
        /// </summary>
        VT_LPSTR = 30,

        /// <summary>
        /// FILETIME
        /// </summary>
        VT_FILETIME = 64,
    }
}
