using RS.Widgets.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interop
{
    public class WeakReferenceList : CopyOnWriteList, IEnumerable
    {
        public int Count
        {
            get
            {
                int num = 0;
                lock (base.SyncRoot)
                {
                    return base.LiveList.Count;
                }
            }
        }

        public WeakReferenceList()
            : base(null)
        {
        }

        public WeakReferenceList(object syncRoot)
            : base(syncRoot)
        {
        }

        public WeakReferenceListEnumerator GetEnumerator()
        {
            return new WeakReferenceListEnumerator(base.List);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(object item)
        {
            lock (base.SyncRoot)
            {
                int num = FindWeakReference(item);
                if (num >= 0)
                {
                    return true;
                }

                return false;
            }
        }

        public override bool Add(object obj)
        {
            return Add(obj, skipFind: false);
        }

        public bool Add(object obj, bool skipFind)
        {
            lock (base.SyncRoot)
            {
                if (skipFind)
                {
                    if (base.LiveList.Count == base.LiveList.Capacity)
                    {
                        Purge();
                    }
                }
                else if (FindWeakReference(obj) >= 0)
                {
                    return false;
                }

                return Internal_Add(new WeakReference(obj));
            }
        }

        public override bool Remove(object obj)
        {
            lock (base.SyncRoot)
            {
                int num = FindWeakReference(obj);
                if (num < 0)
                {
                    return false;
                }

                return RemoveAt(num);
            }
        }

        public bool Insert(int index, object obj)
        {
            lock (base.SyncRoot)
            {
                int num = FindWeakReference(obj);
                if (num >= 0)
                {
                    return false;
                }

                return Internal_Insert(index, new WeakReference(obj));
            }
        }

        private int FindWeakReference(object obj)
        {
            bool flag = true;
            int result = -1;
            while (flag)
            {
                flag = false;
                ArrayList liveList = base.LiveList;
                for (int i = 0; i < liveList.Count; i++)
                {
                    WeakReference weakReference = (WeakReference)liveList[i];
                    if (weakReference.IsAlive)
                    {
                        if (obj == weakReference.Target)
                        {
                            result = i;
                            break;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    Purge();
                }
            }

            return result;
        }

        private void Purge()
        {
            ArrayList liveList = base.LiveList;
            int count = liveList.Count;
            int i;
            for (i = 0; i < count; i++)
            {
                WeakReference weakReference = (WeakReference)liveList[i];
                if (!weakReference.IsAlive)
                {
                    break;
                }
            }

            if (i >= count)
            {
                return;
            }

            DoCopyOnWriteCheck();
            liveList = base.LiveList;
            for (int j = i + 1; j < count; j++)
            {
                WeakReference weakReference2 = (WeakReference)liveList[j];
                if (weakReference2.IsAlive)
                {
                    liveList[i++] = liveList[j];
                }
            }

            if (i < count)
            {
                liveList.RemoveRange(i, count - i);
                int num = i << 1;
                if (num < liveList.Capacity)
                {
                    liveList.Capacity = num;
                }
            }
        }
    }
}
