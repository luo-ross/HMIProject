using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    public enum ColorSpace : uint
    {
        SPACE_XYZ = 0x58595A20,  // = 'XYZ '
        SPACE_Lab = 0x4C616220,  // = 'Lab '
        SPACE_Luv = 0x4C757620,  // = 'Luv '
        SPACE_YCbCr = 0x59436272,  // = 'YCbr'
        SPACE_Yxy = 0x59787920,  // = 'Yxy '
        SPACE_RGB = 0x52474220,  // = 'RGB '
        SPACE_GRAY = 0x47524159,  // = 'GRAY'
        SPACE_HSV = 0x48535620,  // = 'HSV '
        SPACE_HLS = 0x484C5320,  // = 'HLS '
        SPACE_CMYK = 0x434D594B,  // = 'CMYK'
        SPACE_CMY = 0x434D5920,  // = 'CMY '
        SPACE_2_CHANNEL = 0x32434C52,  // = '2CLR'
        SPACE_3_CHANNEL = 0x33434C52,  // = '3CLR'
        SPACE_4_CHANNEL = 0x34434C52,  // = '4CLR'
        SPACE_5_CHANNEL = 0x35434C52,  // = '5CLR'
        SPACE_6_CHANNEL = 0x36434C52,  // = '6CLR'
        SPACE_7_CHANNEL = 0x37434C52,  // = '7CLR'
        SPACE_8_CHANNEL = 0x38434C52,  // = '8CLR'

        // These are not in icm.h but were present in our original
        // implementation. We don't know if these actually exist 
        // but we're going to leave them anyway for compat.
        SPACE_9_CHANNEL = 0x39434C52,  // = '9CLR'
        SPACE_A_CHANNEL = 0x41434C52,  // = 'ACLR'
        SPACE_B_CHANNEL = 0x42434C52,  // = 'BCLR'
        SPACE_C_CHANNEL = 0x43434C52,  // = 'CCLR'
        SPACE_D_CHANNEL = 0x44434C52,  // = 'DCLR'
        SPACE_E_CHANNEL = 0x45434C52,  // = 'ECLR'
        SPACE_F_CHANNEL = 0x46434C52,  // = 'FCLR'
        SPACE_sRGB = 0x73524742   // = 'sRGB'
    };
}
