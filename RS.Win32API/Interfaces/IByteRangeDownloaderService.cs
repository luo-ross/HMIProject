using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Interfaces
{
    // <summary>
    // This interface is used to provide ByteRangeDownloader in Windows Client Applications
    // The unmanaged ByteWrapper communicates with ByteRangeDownloader through this
    // interface using COM interop.
    // </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("e7b92912-c7ca-4629-8f39-0f537cfab57e")]
    public interface IByteRangeDownloaderService
    {
        // <summary>
        // Initialize the downloader for byte range request
        // </summary>
        // <param name="url">url to be downloaded</param>
        // <param name="tempFile">temporary file where the downloaded bytes should be saved</param>
        // <param name="eventHandle">event handle to be raised when a byte range request is done</param>
        /// <SecurityNote>
        /// Critical : Accepts critical SafeHandle argument
        /// </SecurityNote>
        [SecurityCritical]
        void InitializeByteRangeDownloader(
            [MarshalAs(UnmanagedType.LPWStr)] string url,
            [MarshalAs(UnmanagedType.LPWStr)] string tempFile,
            SafeWaitHandle eventHandle);

        // <summary>
        // Make HTTP byte range web request
        // </summary>
        // <param name="byteRanges">byte ranges to be downloaded; byteRanges is one dimensional
        // array consisting pairs of offset and length</param>
        // <param name="size">number of elements in byteRanges</param>
        void RequestDownloadByteRanges(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] byteRanges,
            int size);

        // <summary>
        // Get the byte ranges that are downloaded
        // </summary>
        // <param name="byteRanges">byte ranges that are downloaded; byteRanges is one dimensional
        // array consisting pairs of offset and length</param>
        // <param name="size">numbe of elements in byteRanges</param>
        void GetDownloadedByteRanges(
            [MarshalAs(UnmanagedType.LPArray)] out int[] byteRanges,
            [MarshalAs(UnmanagedType.I4)] out int size);

        // <summary>
        // Release the byte range downloader
        // </summary>
        void ReleaseByteRangeDownloader();
    }
}
