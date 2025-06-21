using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Standard
{
    public static partial class CLSID
    {
        public static T CoCreateInstance<T>(string clsid)
        {
            return (T)System.Activator.CreateInstance(System.Type.GetTypeFromCLSID(new System.Guid(clsid)));
        }

        /// <summary>CLSID_TaskbarList</summary>
        /// <remarks>IID_ITaskbarList</remarks>
        public const string TaskbarList = "56FDF344-FD6D-11d0-958A-006097C9A090";
        /// <summary>CLSID_EnumerableObjectCollection</summary>
        /// <remarks>IID_IEnumObjects.</remarks>
        public const string EnumerableObjectCollection = "2d3468c1-36a7-43b6-ac24-d3f02fd9607a";
        /// <summary>CLSID_ShellLink</summary>
        /// <remarks>IID_IShellLink</remarks>
        public const string ShellLink = "00021401-0000-0000-C000-000000000046";

        #region Win7 CLSIDs

        /// <summary>CLSID_DestinationList</summary>
        /// <remarks>IID_ICustomDestinationList</remarks>
        public const string DestinationList = "77f10cf0-3db5-4966-b520-b7c54fd35ed6";
        /// <summary>CLSID_ApplicationDestinations</summary>
        /// <remarks>IID_IApplicationDestinations</remarks>
        public const string ApplicationDestinations = "86c14003-4d6b-4ef3-a7b4-0506663b2e68";
        /// <summary>CLSID_ApplicationDocumentLists</summary>
        /// <remarks>IID_IApplicationDocumentLists</remarks>
        public const string ApplicationDocumentLists = "86bec222-30f2-47e0-9f25-60d11cd75c28";

        #endregion
    }
}
