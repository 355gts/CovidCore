namespace Covid.Common.Model
{
    public class ResultStatus<T>
    {
        public bool Success { get; set; }

        public T Value { get; set; }

        public string ErrorMessage { get; set; }

        public ResultStatus(bool success)
        {
            Success = success;
        }

        public ResultStatus(bool success, T value)
        {
            Success = success;
            Value = value;
        }

        private ResultStatus()
        {

        }
    }
}
