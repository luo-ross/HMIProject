using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.IO
{
    public static class Native
    {
        /// <summary>
        /// Initialize the constants
        /// </summary>
        /// <SecurityNote>
        ///     Critical: Critical as this code invokes Marshal.SizeOf which uses LinkDemand for UnmanagedCode permission.
        ///     TreatAsSafe: The method doesn't take any user inputs. It only pre-computes the size of our internal types.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        static Native()
        {
            SizeOfInt = (uint)Marshal.SizeOf(typeof(int));
            SizeOfUInt = (uint)Marshal.SizeOf(typeof(uint));
            SizeOfUShort = (uint)Marshal.SizeOf(typeof(ushort));
            SizeOfByte = (uint)Marshal.SizeOf(typeof(byte));
            SizeOfFloat = (uint)Marshal.SizeOf(typeof(float));
            SizeOfDouble = (uint)Marshal.SizeOf(typeof(double));
            SizeOfGuid = (uint)Marshal.SizeOf(typeof(Guid));
            SizeOfDecimal = (uint)Marshal.SizeOf(typeof(decimal));
        }

        public static readonly uint SizeOfInt;      // Size of an int
        public static readonly uint SizeOfUInt;     // Size of an unsigned int
        public static readonly uint SizeOfUShort;   // Size of an unsigned short
        public static readonly uint SizeOfByte;     // Size of a byte
        public static readonly uint SizeOfFloat;    // Size of a float
        public static readonly uint SizeOfDouble;   // Size of a double
        public static readonly uint SizeOfGuid;    // Size of a GUID
        public static readonly uint SizeOfDecimal; // Size of a VB-style Decimal

        public const int BitsPerByte = 8;    // number of bits in a byte
        public const int BitsPerShort = 16;    // number of bits in one short - 2 bytes
        public const int BitsPerInt = 32;    // number of bits in one integer - 4 bytes
        public const int BitsPerLong = 64;    // number of bits in one long - 8 bytes


        // since casting from floats have mantisaa components,
        //      casts from float to int are not constrained by
        //      Int32.MaxValue, but by the maximum float value
        //      whose mantissa component is still within range
        //      of an integer. Anything larger will cause an overflow.
        public const int MaxFloatToIntValue = 2147483584 - 1; // 2.14748e+009
    }
}
