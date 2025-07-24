using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client
{
    public class OleDataSourceEx
    {
        public IDragSourceHelper DragSourceHelper { get; set; }
        public IDragSourceHelper2 DragSourceHelper2 { get; set; }

        public OleDataSourceEx()
        {
            var clsid = new Guid("4657278A-411B-11D2-839A-00C04FD918D0"); // CLSID_DragDropHelper
            var iid = typeof(IDragSourceHelper).GUID;
            object helperObj = null;
            int hr = CoCreateInstance(ref clsid, null, 1 /*CLSCTX_INPROC_SERVER*/, ref iid, out helperObj);
            if (hr == 0 && helperObj != null)
            {
                DragSourceHelper = (IDragSourceHelper)helperObj;
                var iid2 = typeof(IDragSourceHelper2).GUID;
                IntPtr ppv;
                hr = Marshal.QueryInterface(Marshal.GetIUnknownForObject(DragSourceHelper), ref iid2, out ppv);
                if (hr == 0 && ppv != IntPtr.Zero)
                {
                    DragSourceHelper2 = (IDragSourceHelper2)Marshal.GetObjectForIUnknown(ppv);
                    Marshal.Release(ppv);
                    Marshal.ReleaseComObject(DragSourceHelper);
                    DragSourceHelper = DragSourceHelper2;
                }
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(
            ref Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
            int dwClsContext,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv);

        [ComImport, Guid("DE5BF786-477A-11D2-839D-00C04FD918D0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDragSourceHelper
        {
            void InitializeFromBitmap(ref SHDRAGIMAGE pshdi, System.Runtime.InteropServices.ComTypes.IDataObject pDataObject);
            void InitializeFromWindow(IntPtr hwnd, ref System.Drawing.Point ppt, System.Runtime.InteropServices.ComTypes.IDataObject pDataObject);
        }

        [ComImport, Guid("83E07D0D-0C5E-40A0-B0D5-CE9C6E3C8B6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDragSourceHelper2 : IDragSourceHelper
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHDRAGIMAGE
        {
            public System.Drawing.Size sizeDragImage;
            public System.Drawing.Point ptOffset;
            public IntPtr hbmpDragImage;
            public int crColorKey;
        }
    }
}
