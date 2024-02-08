namespace XeSharp.Helpers
{
    public class ExceptionHelper
    {
        /// <summary>
        /// Handles an <see cref="OperationCanceledException"/>.
        /// </summary>
        /// <param name="in_action">The action to try.</param>
        /// <param name="in_callback">The action to perform if we caught the exception.</param>
        public static void OperationCancelledHandler(Action in_action, Action in_callback = null)
        {
            try
            {
                in_action();
            }
            catch (OperationCanceledException)
            {
                if (in_callback == null)
                    return;

                in_callback();
            }
        }
    }
}
