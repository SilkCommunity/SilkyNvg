using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;

namespace SilkyNvg.Utils
{
    [Serializable]
    internal class DataAccessibleArrayList<T> : IList<T>, IList, IReadOnlyList<T>
    {

        private const int DefaultCapacity = 64;

        private static readonly T[] EmptyArray = Array.Empty<T>();
        
        private T[] _items;
        private int _version;
        
        [ContractPublicPropertyName("Count")]
        private int _size;

        [NonSerialized]
        private object _syncRoot;

        internal ReadOnlySpan<T> Data => _items;

        internal DataAccessibleArrayList()
        {
            _items = EmptyArray;
        }

        internal DataAccessibleArrayList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "must not be negative");
            }

            _items = capacity == 0 ? EmptyArray : new T[capacity];
        }

        internal DataAccessibleArrayList(ICollection<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int count = collection.Count;
            if (count == 0)
            {
                _items = EmptyArray;
            }
            else
            {
                _items = new T[count];
                collection.CopyTo(_items, 0);
                _size = count;
            }
        }

        internal int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "capacity too small!");
                }

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        var newItems = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, 0, newItems, 0, _size);
                        }
                        _items = newItems;
                    }
                    else
                    {
                        _items = EmptyArray;
                    }
                }
            }
        }

        public int Count => _size;

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot is null)
                {
                    System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        public T this[int index]
        {
            get => (uint)index >= (uint)_size ? throw new ArgumentOutOfRangeException(nameof(index)) : _items[index];
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                _items[index] = value;
                _version++;
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if ((default(T) != null) && value == null)
                {
                    throw new NoNullAllowedException();
                }

                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException(nameof(value));
                }
            }
        }

        private void EnsureCapacity(int capacity)
        {
            if (_items.Length < capacity)
            {
                int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
                newCapacity = (int)Math.Min((uint)newCapacity, 2e9); // Allow max. 2 billion elements
                newCapacity = Math.Max(newCapacity, capacity); // except we need more, then let fail
                Capacity = newCapacity;
            }
        }

        public void Add(T item)
        {
            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }

            _items[_size++] = item;
            _version++;
        }

        int IList.Add(object value)
        {
            if ((default(T) != null) && value == null)
            {
                throw new NoNullAllowedException();
            }

            try
            {
                Add((T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return Count - 1;
        }

        public void AddRange(ICollection<T> collection)
        {
            InsertRange(_size, collection);
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }

            _version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < _size; i++)
                {
                    if (_items[i] == null)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                var c = EqualityComparer<T>.Default;
                for (int i = 0; i < _size; i++)
                {
                    if (c.Equals(_items[i], item))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection)this).CopyTo(array, 0);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array.Rank != 1)
            {
                throw new ArgumentException("Rank multi dimension not supported", nameof(array));
            }

            try
            {
                Array.Copy(_items, 0, array, index, _size);
            }
            catch (ArrayTypeMismatchException e)
            {
                throw new ArgumentException(nameof(array), e);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public DataAccessibleArrayList<T> GetRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "negative indices not allowed");;
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "needs non-negative count");
            }

            if (_size - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count too large");
            }

            var list = new DataAccessibleArrayList<T>(count);
            Array.Copy(_items, index, list._items, 0, count);
            list._size = count;
            return list;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, _size);
        }

        public int IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T)value);
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "insertion index out of range");
            }

            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }

            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + 1, _size - index);
            }

            _items[index] = item;
            _size++;
            _version++;
        }

        public void Insert(int index, object value)
        {
            if ((default(T) != null) && value == null)
            {
                throw new NoNullAllowedException();
            }

            try
            {
                Insert(index, (T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("invalid type", nameof(value));
            }
        }

        public void InsertRange(int index, ICollection<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "insertion index out of range");
            }

            int count = collection.Count;
            if (count > 0)
            {
                EnsureCapacity(_size + count);
                if (index < _size)
                {
                    Array.Copy(_items, index, _items, index + count, _size - index);
                }

                if (ReferenceEquals(this, collection))
                {
                    Array.Copy(_items, 0, _items, index, index);
                    Array.Copy(_items, index + count, _items, index * 2, _size - index);
                }
                else
                {
                    var itemsToInsert = new T[count];
                    collection.CopyTo(itemsToInsert, 0);
                    itemsToInsert.CopyTo(_items, index);
                }
                _size += count;
            }
            else
            {
                using (IEnumerator<T> e = collection.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        Insert(index++, e.Current);
                    }
                }
            }

            _version++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                Remove((T)value);
            }
        }

        public void RemoveAt(int index)
        {
            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "index out of range");
            }

            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }

            _items[_size] = default;
            _version++;
        }

        public T[] ToArray()
        {
            var array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        public void TrimExcess()
        {
            int threshold = (int)(((double)_items.Length) * 0.9);
            if (_size < threshold)
            {
                Capacity = _size;
            }
        }

        private struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly DataAccessibleArrayList<T> _list;
            private readonly int _version;
            
            private int _index;
            private T _current;

            internal Enumerator(DataAccessibleArrayList<T> list)
            {
                _list = list;
                _index = 0;
                _version = list._version;
                _current = default;
            }

            public T Current => _current;

            object IEnumerator.Current => Current;

            public void Dispose() { }

            private bool MoveNextRare()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("List modified!");
                }

                _index = _list._size + 1;
                _current = default;
                return false;
            }
            
            public bool MoveNext()
            {
                DataAccessibleArrayList<T> localList = _list;

                if (_version == localList._version && (uint)_index < (uint)localList._size)
                {
                    _current = localList._items[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            public void Reset()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("List modified!");
                }
                
                _index = 0;
                _current = default;
            }
        }

        private static bool IsCompatibleObject(object obj)
        {
            return ((obj is T) || (obj == null && default(T) == null));
        }
        
    }
}