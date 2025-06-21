using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interop
{
    public class CopyOnWriteList
    {
        private object _syncRoot;

        private ArrayList _LiveList = new ArrayList();

        private ArrayList _readonlyWrapper;

        public ArrayList List
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_readonlyWrapper == null)
                    {
                        _readonlyWrapper = ArrayList.ReadOnly(_LiveList);
                    }

                    return _readonlyWrapper;
                }
            }
        }

        protected object SyncRoot => _syncRoot;

        protected ArrayList LiveList => _LiveList;

        public CopyOnWriteList()
            : this(null)
        {
        }

        public CopyOnWriteList(object syncRoot)
        {
            if (syncRoot == null)
            {
                syncRoot = new object();
            }

            _syncRoot = syncRoot;
        }

        public virtual bool Add(object obj)
        {
            lock (_syncRoot)
            {
                int num = Find(obj);
                if (num >= 0)
                {
                    return false;
                }

                return Internal_Add(obj);
            }
        }

        public virtual bool Remove(object obj)
        {
            lock (_syncRoot)
            {
                int num = Find(obj);
                if (num < 0)
                {
                    return false;
                }

                return RemoveAt(num);
            }
        }

        protected bool Internal_Add(object obj)
        {
            DoCopyOnWriteCheck();
            _LiveList.Add(obj);
            return true;
        }

        protected bool Internal_Insert(int index, object obj)
        {
            DoCopyOnWriteCheck();
            _LiveList.Insert(index, obj);
            return true;
        }

        private int Find(object obj)
        {
            for (int i = 0; i < _LiveList.Count; i++)
            {
                if (obj == _LiveList[i])
                {
                    return i;
                }
            }

            return -1;
        }

        protected bool RemoveAt(int index)
        {
            if (index < 0 || index >= _LiveList.Count)
            {
                return false;
            }

            DoCopyOnWriteCheck();
            _LiveList.RemoveAt(index);
            return true;
        }

        protected void DoCopyOnWriteCheck()
        {
            if (_readonlyWrapper != null)
            {
                _LiveList = (ArrayList)_LiveList.Clone();
                _readonlyWrapper = null;
            }
        }
    }
}
