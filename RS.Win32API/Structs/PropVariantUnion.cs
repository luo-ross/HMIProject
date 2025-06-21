using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariantUnion
    {
        /// <summary>
        /// CHAR
        /// </summary>
        [FieldOffset(0)]
        public sbyte cVal;

        /// <summary>
        /// UCHAR
        /// </summary>
        [FieldOffset(0)]
        public byte bVal;

        /// <summary>
        /// SHORT
        /// </summary>
        [FieldOffset(0)]
        public short iVal;

        /// <summary>
        /// USHORT
        /// </summary>
        [FieldOffset(0)]
        public ushort uiVal;

        /// <summary>
        /// LONG
        /// </summary>
        [FieldOffset(0)]
        public int lVal;

        /// <summary>
        /// ULONG
        /// </summary>
        [FieldOffset(0)]
        public uint ulVal;

        /// <summary>
        /// INT
        /// </summary>
        [FieldOffset(0)]
        public int intVal;

        /// <summary>
        /// UINT
        /// </summary>
        [FieldOffset(0)]
        public uint uintVal;

        /// <summary>
        /// LARGE_INTEGER
        /// </summary>
        [FieldOffset(0)]
        public Int64 hVal;

        /// <summary>
        /// ULARGE_INTEGER
        /// </summary>
        [FieldOffset(0)]
        public UInt64 uhVal;

        /// <summary>
        /// FLOAT
        /// </summary>
        [FieldOffset(0)]
        public float fltVal;

        /// <summary>
        /// DOUBLE
        /// </summary>
        [FieldOffset(0)]
        public double dblVal;

        /// <summary>
        /// VARIANT_BOOL
        /// </summary>
        [FieldOffset(0)]
        public short boolVal;

        /// <summary>
        /// SCODE
        /// </summary>
        [FieldOffset(0)]
        public int scode;

        /// <summary>
        /// CY
        /// </summary>
        [FieldOffset(0)]
        public CY cyVal;

        /// <summary>
        /// DATE
        /// </summary>
        [FieldOffset(0)]
        public double date;

        /// <summary>
        /// FILETIME
        /// </summary>
        [FieldOffset(0)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;


        /// <summary>
        /// CLSID*   
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puuid;

        /// <summary>
        /// CLIPDATA*    
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pclipdata;

        /// <summary>
        /// BSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr bstrVal;

        /// <summary>
        /// BSTRBLOB   
        /// </summary>
        [FieldOffset(0)]
        public BSTRBLOB bstrblobVal;

        /// <summary>
        /// BLOB
        /// </summary>
        [FieldOffset(0)]
        public BLOB blob;

        /// <summary>
        /// LPSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pszVal;

        /// <summary>
        /// LPWSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pwszVal;

        /// <summary>
        /// IUnknown*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr punkVal;

        /// <summary>
        /// IDispatch*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdispVal;

        /// <summary>
        /// IStream*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pStream;

        /// <summary>
        /// IStorage*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pStorage;

        /// <summary>
        /// LPVERSIONEDSTREAM
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pVersionedStream;

        /// <summary>
        /// LPSAFEARRAY 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr parray;

        /// <summary>
        /// Placeholder for
        /// CAC, CAUB, CAI, CAUI, CAL, CAUL, CAH, CAUH; CAFLT,
        /// CADBL, CABOOL, CASCODE, CACY, CADATE, CAFILETIME, 
        /// CACLSID, CACLIPDATA, CABSTR, CABSTRBLOB, 
        /// CALPSTR, CALPWSTR, CAPROPVARIANT 
        /// </summary>
        [FieldOffset(0)]
        public CArray cArray;

        /// <summary>
        /// CHAR*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pcVal;

        /// <summary>
        /// UCHAR* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pbVal;

        /// <summary>
        /// SHORT* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr piVal;

        /// <summary>
        /// USHORT* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puiVal;

        /// <summary>
        /// LONG* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr plVal;

        /// <summary>
        /// ULONG* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pulVal;

        /// <summary>
        /// INT* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pintVal;

        /// <summary>
        /// UINT* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puintVal;

        /// <summary>
        /// FLOAT* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pfltVal;

        /// <summary>
        /// DOUBLE* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdblVal;

        /// <summary>
        /// VARIANT_BOOL* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pboolVal;

        /// <summary>
        /// DECIMAL* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdecVal;

        /// <summary>
        /// SCODE* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pscode;

        /// <summary>
        /// CY* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pcyVal;

        /// <summary>
        /// DATE* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdate;

        /// <summary>
        /// BSTR*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pbstrVal;

        /// <summary>
        /// IUnknown** 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr ppunkVal;

        /// <summary>
        /// IDispatch** 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr ppdispVal;

        /// <summary>
        /// LPSAFEARRAY* 
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pparray;

        /// <summary>
        /// PROPVARIANT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pvarVal;
    }
}
