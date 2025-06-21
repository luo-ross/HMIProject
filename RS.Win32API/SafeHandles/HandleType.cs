using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public class HandleType
    {
        public readonly string name;

        private int initialThreshHold;
        private int threshHold;
        private int handleCount;
        private readonly int deltaPercent;

        /// <devdoc>
        ///     Creates a new handle type.
        /// </devdoc>
        public HandleType(string name, int expense, int initialThreshHold)
        {
            this.name = name;
            this.initialThreshHold = initialThreshHold;
            threshHold = initialThreshHold;
            deltaPercent = 100 - expense;
        }

        /// <devdoc>
        ///     Adds a handle to this handle type for monitoring.
        /// </devdoc>
        public void Add()
        {
            bool performCollect = false;

            lock (this)
            {
                handleCount++;
                performCollect = NeedCollection();

                if (!performCollect)
                {
                    return;
                }
            }

            if (performCollect)
            {

                GC.Collect();
                // We just performed a GC.  If the main thread is in a tight
                // loop there is a this will cause us to increase handles forever and prevent handle collector
                // from doing its job.  Yield the thread here.  This won't totally cause
                // a finalization pass but it will effectively elevate the priority
                // of the finalizer thread just for an instant.  But how long should
                // we sleep?  We base it on how expensive the handles are because the
                // more expensive the handle, the more critical that it be reclaimed.
                int sleep = (100 - deltaPercent) / 4;
                Thread.Sleep(sleep);
            }
        }


        /// <devdoc>
        ///     Determines if this handle type needs a garbage collection pass.
        /// </devdoc>
        public bool NeedCollection()
        {

            if (handleCount > threshHold)
            {
                threshHold = handleCount + handleCount * deltaPercent / 100;
                return true;
            }

            // If handle count < threshHold, we don't
            // need to collect, but if it 10% below the next lowest threshhold we
            // will bump down a rung.  We need to choose a percentage here or else
            // we will oscillate.
            //
            int oldThreshHold = 100 * threshHold / (100 + deltaPercent);
            if (oldThreshHold >= initialThreshHold && handleCount < (int)(oldThreshHold * .9F))
            {
                threshHold = oldThreshHold;
            }

            return false;
        }

        /// <devdoc>
        ///     Removes the given handle from our monitor list.
        /// </devdoc>
        public void Remove()
        {
            lock (this)
            {
                handleCount--;

                handleCount = Math.Max(0, handleCount);
            }
        }
    }
}
