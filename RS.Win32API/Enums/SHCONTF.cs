using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <summary>
    /// IShellFolder::EnumObjects grfFlags bits.  Also called SHCONT
    /// </summary>
    public enum SHCONTF
    {
        CHECKING_FOR_CHILDREN = 0x0010,   // hint that client is checking if (what) child items the folder contains - not all details (e.g. short file name) are needed
        FOLDERS = 0x0020,   // only want folders enumerated (SFGAO_FOLDER)
        NONFOLDERS = 0x0040,   // include non folders (items without SFGAO_FOLDER)
        INCLUDEHIDDEN = 0x0080,   // show items normally hidden (items with SFGAO_HIDDEN)
        INIT_ON_FIRST_NEXT = 0x0100,   // DEFUNCT - this is always assumed
        NETPRINTERSRCH = 0x0200,   // hint that client is looking for printers
        SHAREABLE = 0x0400,   // hint that client is looking sharable resources (local drives or hidden root shares)
        STORAGE = 0x0800,   // include all items with accessible storage and their ancestors
        NAVIGATION_ENUM = 0x1000,   // mark child folders to indicate that they should provide a "navigation" enumeration by default
        FASTITEMS = 0x2000,   // hint that client is only interested in items that can be enumerated quickly
        FLATLIST = 0x4000,   // enumerate items as flat list even if folder is stacked
        ENABLE_ASYNC = 0x8000,   // inform enumerator that client is listening for change notifications so enumerator does not need to be complete, items can be reported via change notifications
    }
}
