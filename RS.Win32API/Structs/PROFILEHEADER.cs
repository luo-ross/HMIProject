using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PROFILEHEADER
    {
        public uint phSize;                  // profile size in bytes
        public uint phCMMType;               // CMM for this profile
        public uint phVersion;               // profile format version number
        public uint phClass;                 // type of profile
        public ColorSpace phDataColorSpace;  // color space of data
        public uint phConnectionSpace;       // PCS
        public uint phDateTime_0;            // date profile was created
        public uint phDateTime_1;            // date profile was created
        public uint phDateTime_2;            // date profile was created
        public uint phSignature;             // magic number ("Reserved for internal use.")
        public uint phPlatform;              // primary platform
        public uint phProfileFlags;          // various bit settings
        public uint phManufacturer;          // device manufacturer
        public uint phModel;                 // device model number
        public uint phAttributes_0;          // device attributes
        public uint phAttributes_1;          // device attributes
        public uint phRenderingIntent;       // rendering intent
        public uint phIlluminant_0;          // profile illuminant
        public uint phIlluminant_1;          // profile illuminant
        public uint phIlluminant_2;          // profile illuminant
        public uint phCreator;               // profile creator
        public fixed byte phReserved[44];
    };

}
