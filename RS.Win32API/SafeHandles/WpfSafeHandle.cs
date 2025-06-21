using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public abstract class WpfSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private int _collectorId;

       
        protected WpfSafeHandle(bool ownsHandle, int collectorId) : base(ownsHandle)
        {
            HandleCollector.Add(collectorId);
            _collectorId = collectorId;
        }

       
        protected override void Dispose(bool disposing)
        {
            HandleCollector.Remove(_collectorId);
            base.Dispose(disposing);
        }

      
    }
}
