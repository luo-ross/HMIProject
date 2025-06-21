using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Structs
{
    public struct WeakReferenceListEnumerator : IEnumerator
    {
        private int _i;

        private ArrayList _List;

        private object _StrongReference;

        object IEnumerator.Current => Current;

        public object Current
        {
            get
            {
                if (_StrongReference == null)
                {
                    throw new InvalidOperationException("Enumerator_VerifyContext");
                }

                return _StrongReference;
            }
        }

        public WeakReferenceListEnumerator(ArrayList List)
        {
            _i = 0;
            _List = List;
            _StrongReference = null;
        }

        public bool MoveNext()
        {
            object obj = null;
            while (_i < _List.Count)
            {
                WeakReference weakReference = (WeakReference)_List[_i++];
                obj = weakReference.Target;
                if (obj != null)
                {
                    break;
                }
            }

            _StrongReference = obj;
            return obj != null;
        }

        public void Reset()
        {
            _i = 0;
            _StrongReference = null;
        }
    }
}
