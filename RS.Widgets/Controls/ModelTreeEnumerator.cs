using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Controls
{
    public abstract class ModelTreeEnumerator : IEnumerator
    {
        private int _index = -1;

        private object _content;

        object IEnumerator.Current => Current;

        protected object Content => _content;

        protected int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        protected virtual object Current
        {
            get
            {
                if (_index == 0)
                {
                    return _content;
                }

                throw new InvalidOperationException("EnumeratorInvalidOperation");
            }
        }

        protected abstract bool IsUnchanged { get; }

        internal ModelTreeEnumerator(object content)
        {
            _content = content;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        protected virtual bool MoveNext()
        {
            if (_index < 1)
            {
                _index++;
                if (_index == 0)
                {
                    VerifyUnchanged();
                    return true;
                }
            }

            return false;
        }

        protected virtual void Reset()
        {
            VerifyUnchanged();
            _index = -1;
        }

        protected void VerifyUnchanged()
        {
            if (!IsUnchanged)
            {
                throw new InvalidOperationException("EnumeratorVersionChanged");
            }
        }
    }
}
