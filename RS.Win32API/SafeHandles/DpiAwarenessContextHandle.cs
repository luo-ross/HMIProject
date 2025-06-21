using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public class DpiAwarenessContextHandle : SafeHandle, IEquatable<IntPtr>, IEquatable<DpiAwarenessContextHandle>, IEquatable<DpiAwarenessContextValue>
    {
        public override bool IsInvalid => true;

        public static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_UNAWARE { get; }

        public static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_SYSTEM_AWARE { get; }

        public static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE { get; }

        public static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 { get; }

        public static Dictionary<DpiAwarenessContextValue, IntPtr> WellKnownContextValues { get; }

        static DpiAwarenessContextHandle()
        {
            WellKnownContextValues = new Dictionary<DpiAwarenessContextValue, IntPtr>
        {
            {
                DpiAwarenessContextValue.Unaware,
                new IntPtr(-1)
            },
            {
                DpiAwarenessContextValue.SystemAware,
                new IntPtr(-2)
            },
            {
                DpiAwarenessContextValue.PerMonitorAware,
                new IntPtr(-3)
            },
            {
                DpiAwarenessContextValue.PerMonitorAwareVersion2,
                new IntPtr(-4)
            }
        };
            DPI_AWARENESS_CONTEXT_UNAWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.Unaware]);
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.SystemAware]);
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.PerMonitorAware]);
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.PerMonitorAwareVersion2]);
        }

        public DpiAwarenessContextHandle()
            : base(IntPtr.Zero, ownsHandle: false)
        {
        }

        public DpiAwarenessContextHandle(DpiAwarenessContextValue dpiAwarenessContextValue)
            : base(WellKnownContextValues[dpiAwarenessContextValue], ownsHandle: false)
        {
        }

        public DpiAwarenessContextHandle(IntPtr dpiContext)
            : base(dpiContext, ownsHandle: false)
        {
        }

        public DpiAwarenessContextHandle(IntPtr invalidHandleValue, bool ownsHandle)
            : base(invalidHandleValue, ownsHandle: false)
        {
        }

        public static explicit operator DpiAwarenessContextValue(DpiAwarenessContextHandle dpiAwarenessContextHandle)
        {
            foreach (DpiAwarenessContextValue value in Enum.GetValues(typeof(DpiAwarenessContextValue)))
            {
                if (value != 0 && dpiAwarenessContextHandle.Equals(value))
                {
                    return value;
                }
            }
            return DpiAwarenessContextValue.Invalid;
        }


        public bool Equals(DpiAwarenessContextHandle dpiContextHandle)
        {
            return NativeMethods.AreDpiAwarenessContextsEqual(DangerousGetHandle(), dpiContextHandle.DangerousGetHandle());
        }

     
        public bool Equals(IntPtr dpiContext)
        {
            return DpiHelper.AreDpiAwarenessContextsEqual(DangerousGetHandle(), dpiContext);
        }

        public bool Equals(DpiAwarenessContextValue dpiContextEnumValue)
        {
            return Equals(WellKnownContextValues[dpiContextEnumValue]);
        }

        public override bool Equals(object obj)
        {
            if (obj is IntPtr)
            {
                return Equals((IntPtr)obj);
            }

            if (obj is DpiAwarenessContextHandle)
            {
                return Equals((DpiAwarenessContextHandle)obj);
            }

            if (obj is DpiAwarenessContextValue)
            {
                return Equals((DpiAwarenessContextValue)obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((DpiAwarenessContextValue)this).GetHashCode();
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }
}
