using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SilkyNvg.Utils
{
    internal class DataAccessibleArrayList<T> : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IEnumerable, ICollection, IList, ICloneable
    {

        private const int DefaultCapacity = 128;

        private T[] _data;
        private int _count;

        internal DataAccessibleArrayList()
        {
            _data = new T[DefaultCapacity];
            _count = 0;
        }

        internal DataAccessibleArrayList(int initialCapacity)
        {
            if (initialCapacity >= 0)
            {
                _data = new T[initialCapacity];
                _count = 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid initial capacity: {initialCapacity}");
            }
        }

        internal ReadOnlySpan<T> ElementData => _data;

        internal DataAccessibleArrayList(ICollection<T> collection)
        {
            var a = collection.ToArray();
            if ((_count = (int)a.Length) != 0)
            {
                _data = new T[_count];
                Array.Copy(a, _data, _count);
            }
            else
            {
                _data = Array.Empty<T>();
                _count = 0;
            }
        }

        internal void TrimToCount()
        {
            if (_count < _data.Length)
            {
                if (_count == 0)
                {
                    _data = Array.Empty<T>();
                }
                else
                {
                    Array.Resize<T>(ref _data, (int)_count);
                }
            }
        }

        internal void EnsureCapacity(int minCapacity)
        {
            if (minCapacity > _data.Length)
            {
                Grow(minCapacity);
            }
        }
        
        private void Grow(int minCapacity)
        {
            int oldCapacity = _data.Length;
            if (oldCapacity > 0)
            {
                int newCapacity = Math.Max(minCapacity, oldCapacity >> 1) + oldCapacity;
                Array.Resize(ref _data, (int)newCapacity);
            }
            else
            {
                _data = new T[Math.Max(DefaultCapacity, minCapacity)];
            }
        }

        private void Grow()
        {
            Grow(_count + 1);
        }

        public int Count => _count;

        internal bool IsEmpty => _count == 0;

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Contains(object value)
        {
            if (value is T v)
            {
                return Contains(v);
            }

            return false;
        }

        private int IndexOfRange(T item, int start, int end)
        {
            var elements = _data;
            if (item == null)
            {
                for (int i = start; i < end; i++)
                {
                    if (elements[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    if (item.Equals(elements[i]))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int IndexOf(T item)
        {
            return IndexOfRange(item, 0, _count);
        }

        public int IndexOf(object value)
        {
            if (value is T v)
            {
                return IndexOf(v);
            }

            return -1;
        }

        private int LastIndexOfRange(T item, int start, int end)
        {
            var elements = _data;
            if (item == null)
            {
                for (int i = end - 1; i >= start; i--)
                {
                    if (elements[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = end - 1; i >= start; i--)
                {
                    if (item.Equals(elements[i]))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOfRange(item, 0, _count);
        }

        public int LastIndexOf(object value)
        {
            if (value is T v)
            {
                return LastIndexOf(v);
            }

            return -1;
        }

        public object Clone()
        {
            return new DataAccessibleArrayList<T>(this);
        }

        public T[] ToArray()
        {
            var array = new T[_count];
            Array.Copy(_data, array, _count);
            return array;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                _data[index] = value;
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T v)
                {
                    this[index] = v;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot add value of type {value.GetType().Name} to List of type {typeof(T).Name}");
                }
            }
        }

        public void Add(T item)
        {
            if (_count == _data.Length)
            {
                Grow();
            }
            _data[_count++] = item;
        }

        public int Add(object value)
        {
            if (value is T v)
            {
                Add(v);
            }
            else
            {
                string type = value == null ? "null" : value.GetType().Name;
                throw new InvalidOperationException($"Cannot add value of type {type} to List of type {typeof(T).Name}");
            }

            return _count;
        }

        private void RangeCheckForAdd(int index)
        {
            if (index > _count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void Insert(int index, T item)
        {
            RangeCheckForAdd(index);
            int s;
            if ((s = _count) == _data.Length)
            {
                Grow();
            }

            Array.Copy(_data, index, _data, index + 1, s - index);
            _data[index] = item;
            _count = s + 1;
        }

        public void Insert(int index, object value)
        {
            if (value is T v)
            {
                Insert(index, v);
            }
            else
            {
                string type = value == null ? "null" : value.GetType().Name;
                throw new InvalidOperationException($"Cannot insert value of type {type} into List of type {typeof(T).Name}");
            }
        }

        private void FastRemove(int i)
        {
            int newCount;
            if ((newCount = _count - 1) > i)
            {
                Array.Copy(_data, i + 1, _data, i, newCount - i);
            }

            _data[_count = newCount] = default;
        }

        void IList<T>.RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            FastRemove(index);
        }

        void IList.RemoveAt(int index)
        {
            ((IList<T>)this).RemoveAt(index);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is DataAccessibleArrayList<T> other))
            {
                return false;
            }

            if (_count != other._count)
            {
                return false;
            }

            for (int i = 0; i < _count; i++)
            {
                if (!object.Equals(_data[i], other._data[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private int HashCodeRange(int from, int to)
        {
            int hashCode = 1;
            for (int i = from; i < to; i++)
            {
                T item = _data[i];
                hashCode = 31 * hashCode + (item == null ? 0 : item.GetHashCode());
            }

            return hashCode;
        }

        public override int GetHashCode()
        {
            int hash = HashCodeRange(0, _count);
            return hash;
        }

        public bool Remove(T item)
        {
            int count = _count;
            int i = 0;
            if (item == null)
            {
                for (; i < count; i++)
                {
                    if (_data[i] == null)
                    {
                        goto found;
                    }
                }
            }
            else
            {
                for (; i < count; i++)
                {
                    if (item.Equals(_data[i]))
                    {
                        goto found;
                    }
                }
            }

            return false;
            
            found:
            FastRemove(i);
            return true;
        }

        public void Remove(object value)
        {
            if (value is T v)
            {
                _ = Remove(v);
            }
        }

        public void Clear()
        {
            for (int to = _count, i = _count = 0; i < to; i++)
            {
                _data[i] = default;
            }
        }

        internal bool AddRange(int index, ICollection<T> collection)
        {
            RangeCheckForAdd(index);

            T[] a = collection.ToArray();
            int numNew = a.Length;
            if (numNew == 0)
            {
                return false;
            }

            int s;
            if (numNew > _data.Length - (s = _count))
            {
                Grow(s + numNew);
            }

            int numMoved = s - index;
            if (numMoved > 0)
            {
                Array.Copy(_data, index, _data, index + numNew, numMoved);
            }

            Array.Copy(a, 0, _data, index, numNew);
            _count = s + numNew;
            return true;
        }
        
        private void ShiftTailOverGap(int lo, int hi)
        {
            Array.Copy(_data, hi, _data, lo, _count - hi);
            for (int to = _count, i = (_count -= hi - lo); i < to; i++)
            {
                _data[i] = default;
            }
        }

        internal void RemoveRange(int index, int count)
        {
            int from = index;
            int to = index + count;
            if (from > to)
            {
                throw new ArgumentOutOfRangeException($"Index: {index}, Size: {count}");
            }

            ShiftTailOverGap(from, to);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumer(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumer : IEnumerator<T>
        {
            private readonly DataAccessibleArrayList<T> _list;
            
            private int _cursor = -1;

            internal Enumer(DataAccessibleArrayList<T> list)
            {
                _list = list;
            }

            public T Current
            {
                get
                {
                    if (_cursor < 0 || _cursor >= _list._count)
                    {
                        throw new InvalidOperationException("Enumerator has completed");
                    }

                    return _list._data[_cursor];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return ++_cursor < _list._count;
            }

            public void Reset()
            {
                _cursor = -1;
            }

            public void Dispose() { }
        }

        public bool IsFixedSize => false;

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_count > array.Length - arrayIndex)
            {
                throw new InvalidOperationException("Target array to small to receive list data");
            }
            Array.Copy(_data, 0, array, arrayIndex, _count);
        }

        public void CopyTo(Array array, int index)
        {
            if (_count > array.Length - index)
            {
                throw new InvalidOperationException("Target array to small to receive list data");
            }
            _data.CopyTo(array, index);
        }

        public bool IsSynchronized => false;

        public object SyncRoot => false;
        
    }
}