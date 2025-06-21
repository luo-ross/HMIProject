using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.SafeHandles
{
    public static class HandleCollector
    {
        private static HandleType[] handleTypes;
        private static int handleTypeCount = 0;

        private static object handleMutex = new object();

        /// <devdoc>
        ///     Adds the given handle to the handle collector.  This keeps the
        ///     handle on a "hot list" of objects that may need to be garbage
        ///     collected.
        /// </devdoc>
        public static nint Add(nint handle, int type)
        {
            handleTypes[type - 1].Add();
            return handle;
        }

   
        public static SafeHandle Add(SafeHandle handle, int type)
        {
            handleTypes[type - 1].Add();
            return handle;
        }

        public static void Add(int type)
        {
            handleTypes[type - 1].Add();
        }

      
        public static int RegisterType(string typeName, int expense, int initialThreshold)
        {
            lock (handleMutex)
            {
                if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
                {
                    HandleType[] newTypes = new HandleType[handleTypeCount + 10];
                    if (handleTypes != null)
                    {
                        Array.Copy(handleTypes, 0, newTypes, 0, handleTypeCount);
                    }
                    handleTypes = newTypes;
                }

                handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                return handleTypeCount;
            }
        }

     
        public static nint Remove(nint handle, int type)
        {
            handleTypes[type - 1].Remove();
            return handle;
        }

   
        public static SafeHandle Remove(SafeHandle handle, int type)
        {
            handleTypes[type - 1].Remove();
            return handle;
        }

        public static void Remove(int type)
        {
            handleTypes[type - 1].Remove();
        }

      
    }
}
