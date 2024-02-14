namespace XeSharp.Collections.Events
{
    public class RegisterChangedEventArgs<T>(int in_index, T in_oldValue, T in_newValue) : EventArgs
    {
        /// <summary>
        /// The index of the register that was changed.
        /// </summary>
        public int Index { get; } = in_index;

        /// <summary>
        /// The old value of the register
        /// </summary>
        public T OldValue { get; } = in_oldValue;

        /// <summary>
        /// The new value of the register.
        /// </summary>
        public T NewValue { get; } = in_newValue;
    }

    public delegate void RegisterChangedEventHandler<T>(object in_sender, RegisterChangedEventArgs<T> in_args);
}
