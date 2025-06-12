using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    public enum QualityMode
    {
        /// <devdoc>
        ///    Specifies an invalid mode.
        /// </devdoc>
        Invalid = -1,
        /// <devdoc>
        ///    Specifies the default mode.
        /// </devdoc>
        Default = 0,
        /// <devdoc>
        ///    Specifies low quality, high performance
        ///    rendering.
        /// </devdoc>
        Low = 1,             // for apps that need the best performance
        /// <devdoc>
        ///    Specifies high quality, lower performance
        ///    rendering.
        /// </devdoc>
        High = 2             // for apps that need the best rendering quality                                          
    }
}
