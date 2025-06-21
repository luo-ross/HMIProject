using Microsoft.Win32.SafeHandles;
using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
namespace RS.Win32API.SafeHandles
{
    /// <SecurityNote>
    ///   Critical : Base type is critical
    /// </SecurityNote>

    internal sealed class SafeConnectionPointCookie : SafeHandleZeroOrMinusOneIsInvalid
    {
        private IConnectionPoint _cp;
        // handle holds the cookie value.

        /// <SecurityNote>
        ///   Critical : Call critical base ctor
        /// </SecurityNote>


        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IConnectionPoint")]
        public SafeConnectionPointCookie(IConnectionPointContainer target, object sink, Guid eventId)
            : base(true)
        {
            Verify.IsNotNull(target, "target");
            Verify.IsNotNull(sink, "sink");
            Verify.IsNotDefault(eventId, "eventId");

            handle = IntPtr.Zero;

            IConnectionPoint cp = null;
            try
            {
                int dwCookie;
                target.FindConnectionPoint(ref eventId, out cp);
                cp.Advise(sink, out dwCookie);
                if (dwCookie == 0)
                {
                    throw new InvalidOperationException("IConnectionPoint::Advise returned an invalid cookie.");
                }
                handle = new IntPtr(dwCookie);
                _cp = cp;
                cp = null;
            }
            finally
            {
                Utility.SafeRelease(ref cp);
            }
        }

        /// <SecurityNote>
        ///   Critical : Calls critical ReleaseHandle
        /// </SecurityNote>


        public void Disconnect()
        {
            ReleaseHandle();
        }

        /// <SecurityNote>
        ///   Critical : Base method is critical and calls critical methods
        /// </SecurityNote>

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            try
            {
                if (!this.IsInvalid)
                {
                    int dwCookie = handle.ToInt32();
                    handle = IntPtr.Zero;

                    Assert.IsNotNull(_cp);
                    try
                    {
                        _cp.Unadvise(dwCookie);
                    }
                    finally
                    {
                        Utility.SafeRelease(ref _cp);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
