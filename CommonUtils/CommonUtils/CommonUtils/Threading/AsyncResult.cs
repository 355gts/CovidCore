namespace CommonUtils.Threading
{
    public sealed class AsyncResult<T>
    {
        public bool Success { get; }

        public T Value { get; }

        public AsyncResult(bool success, T value)
        {
            Success = success;
            Value = value;
        }
    }

    public static class AsyncResult
    {
        public static AsyncResult<T> Failure<T>()
        {
            return new AsyncResult<T>(false, default(T));
        }

        public static AsyncResult<T> Success<T>(T value)
        {
            return new AsyncResult<T>(true, value);
        }
    }
}
