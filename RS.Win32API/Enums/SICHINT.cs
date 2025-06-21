using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{

    /// <summary>
    /// SHELLITEMCOMPAREHINTF.  SICHINT_*.
    /// </summary>
    public enum SICHINT : uint
    {
        /// <summary>iOrder based on display in a folder view</summary>
        DISPLAY = 0x00000000,
        /// <summary>exact instance compare</summary>
        ALLFIELDS = 0x80000000,
        /// <summary>iOrder based on canonical name (better performance)</summary>
        CANONICAL = 0x10000000,
        TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
    };
}
