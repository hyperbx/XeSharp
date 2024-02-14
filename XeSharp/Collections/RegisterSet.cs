using System.Collections;
using XeSharp.Collections.Events;

namespace XeSharp.Collections
{
    public class RegisterSet<T>(int in_count) : IEnumerable<T> where T : unmanaged
    {
        public readonly T[] Buffer = new T[in_count];

        public int Length => Buffer.Length;

        public event RegisterChangedEventHandler<T> RegisterChanged;

        public T this[int in_index]
        {
            get => ElementAt(in_index);
            set => Set(in_index, value);
        }

        public T ElementAt(int in_index)
        {
            return Buffer[in_index];
        }

        public void Set(int in_index, T in_value)
        {
            if ((object)in_value == (object)Buffer[in_index])
                return;

            var oldValue = Buffer[in_index];

            Buffer[in_index] = in_value;

            RegisterChanged?.Invoke(this, new RegisterChangedEventArgs<T>(in_index, oldValue, in_value));
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
                yield return Buffer[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
