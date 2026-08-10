using System;

namespace SilkyNvg.Utils
{
    internal class DataAccessibleArrayList<T>
        where T : unmanaged
    {

        private T[] _data;

        private int _count;
        private int _capacity;

        internal T[] Data => _data;
        
        internal int Count => _count;
        
        internal DataAccessibleArrayList(int initialCapacity = 0)
        {
            _data = new T[initialCapacity];
            _capacity = initialCapacity;
            _count = 0;
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity > _capacity)
            {
                int newCapacity = _count + capacity + _capacity / 2;
                Array.Resize(ref _data, newCapacity);
                _capacity = newCapacity;
            }
        }

        internal void Add(T item)
        {
            EnsureCapacity(_count + 1);
            _data[_count] = item;
            _count++;
        }

        internal void AddRange(params T[] items)
        {
            EnsureCapacity(_count + items.Length);
            Array.Copy(items, 0, _data, _count, items.Length);
            _count += items.Length;
        }

        internal void Clear()
        {
            _count = 0;
        }

    }
}