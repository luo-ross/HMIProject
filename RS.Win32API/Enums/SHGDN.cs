using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <summary>
    /// IShellFolder::GetDisplayNameOf/SetNameOf uFlags.  Also called SHGDNF.
    /// </summary>
    /// <remarks>
    /// For compatibility with SIGDN, these bits must all sit in the LOW word.
    /// </remarks>
    [Flags]
    public enum SHGDN
    {
        SHGDN_NORMAL = 0x0000,  // default (display purpose)
        SHGDN_INFOLDER = 0x0001,  // displayed under a folder (relative)
        SHGDN_FOREDITING = 0x1000,  // for in-place editing
        SHGDN_FORADDRESSBAR = 0x4000,  // UI friendly parsing name (remove ugly stuff)
        SHGDN_FORPARSING = 0x8000,  // parsing name for ParseDisplayName()
    }
}
