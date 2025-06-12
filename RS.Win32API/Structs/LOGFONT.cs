using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class LOGFONT
    {
        public LOGFONT()
        {
        }
        public LOGFONT(LOGFONT lf)
        {
            if (lf == null)
            {
                throw new ArgumentNullException("lf");
            }

            this.lfHeight = lf.lfHeight;
            this.lfWidth = lf.lfWidth;
            this.lfEscapement = lf.lfEscapement;
            this.lfOrientation = lf.lfOrientation;
            this.lfWeight = lf.lfWeight;
            this.lfItalic = lf.lfItalic;
            this.lfUnderline = lf.lfUnderline;
            this.lfStrikeOut = lf.lfStrikeOut;
            this.lfCharSet = lf.lfCharSet;
            this.lfOutPrecision = lf.lfOutPrecision;
            this.lfClipPrecision = lf.lfClipPrecision;
            this.lfQuality = lf.lfQuality;
            this.lfPitchAndFamily = lf.lfPitchAndFamily;
            this.lfFaceName = lf.lfFaceName;
        }
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }
}
