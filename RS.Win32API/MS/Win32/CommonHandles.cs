using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.MS.Win32
{
    public static class CommonHandles
    {
        static CommonHandles()
        {
        }

        /// <devdoc>
        ///     Handle type for accelerator tables.
        /// </devdoc>
        public static readonly int Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);

        /// <devdoc>
        ///     handle type for cursors.
        /// </devdoc>
        public static readonly int Cursor = HandleCollector.RegisterType("Cursor", 20, 500);

        /// <devdoc>
        ///     Handle type for enhanced metafiles.
        /// </devdoc>
        public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);

        /// <devdoc>
        ///     Handle type for file find handles.
        /// </devdoc>
        public static readonly int Find = HandleCollector.RegisterType("Find", 0, 1000);

        /// <devdoc>
        ///     Handle type for GDI objects.
        /// </devdoc>
        public static readonly int GDI = HandleCollector.RegisterType("GDI", 50, 500);

        /// <devdoc>
        ///     Handle type for HDC's that count against the Win98 limit of five DC's.  HDC's
        ///     which are not scarce, such as HDC's for bitmaps, are counted as GDIHANDLE's.
        /// </devdoc>
        public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2); // wait for 2 dc's before collecting

        /// <devdoc>
        ///     Handle type for icons.
        /// </devdoc>
        public static readonly int Icon = HandleCollector.RegisterType("Icon", 20, 500);

        /// <devdoc>
        ///     Handle type for kernel objects.
        /// </devdoc>
        public static readonly int Kernel = HandleCollector.RegisterType("Kernel", 0, 1000);

        /// <devdoc>
        ///     Handle type for files.
        /// </devdoc>
        public static readonly int Menu = HandleCollector.RegisterType("Menu", 30, 1000);

        /// <devdoc>
        ///     Handle type for windows.
        /// </devdoc>
        public static readonly int Window = HandleCollector.RegisterType("Window", 5, 1000);
    }
}
