using RS.Win32API.SafeHandles;
using System.Runtime.InteropServices;

namespace RS.Win32API.Structs
{

    [StructLayout(LayoutKind.Sequential)]
    public class ICONINFO
    {
        public bool fIcon = false;
        public int xHotspot = 0;
        public int yHotspot = 0;
        public BitmapHandle hbmMask = null;
        public BitmapHandle hbmColor = null;
    }
}
