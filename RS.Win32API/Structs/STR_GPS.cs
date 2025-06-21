using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    /// <summary>
    /// STR_GPS_*
    /// </summary>
    /// <remarks>
    /// When requesting a property store through IShellFolder, you can specify the equivalent of
    /// GPS_DEFAULT by passing in a null IBindCtx parameter.
    ///
    /// You can specify the equivalent of GPS_READWRITE by passing a mode of STGM_READWRITE | STGM_EXCLUSIVE
    /// in the bind context
    ///
    /// Here are the string versions of GPS_ flags, passed to IShellFolder::BindToObject() via IBindCtx::RegisterObjectParam()
    /// These flags are valid when requesting an IPropertySetStorage or IPropertyStore handler
    ///
    /// The meaning of these flags are described above.
    ///
    /// There is no STR_ equivalent for GPS_TEMPORARY because temporary property stores
    /// are provided by IShellItem2 only -- not by the underlying IShellFolder.
    /// </remarks>
    public static class STR_GPS
    {
        public const string HANDLERPROPERTIESONLY = "GPS_HANDLERPROPERTIESONLY";
        public const string FASTPROPERTIESONLY = "GPS_FASTPROPERTIESONLY";
        public const string OPENSLOWITEM = "GPS_OPENSLOWITEM";
        public const string DELAYCREATION = "GPS_DELAYCREATION";
        public const string BESTEFFORT = "GPS_BESTEFFORT";
        public const string NO_OPLOCK = "GPS_NO_OPLOCK";
    }
}
