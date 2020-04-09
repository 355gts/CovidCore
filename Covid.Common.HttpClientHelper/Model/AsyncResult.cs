using System.Runtime.Serialization;

namespace Covid.Common.HttpClientHelper.Model
{
    [DataContract]
    public class AsyncResult<T>
    {
        [DataMember(IsRequired = true)]
        public bool Success { get; set; }

        [DataMember(IsRequired = false)]
        public string ErrorMessage { get; set; }

        [DataMember(IsRequired = true)]
        public T Result { get; set; }

        public AsyncResult(bool success)
        {
            Success = success;
        }

        public AsyncResult(bool success, T result)
        {
            Success = success;
            Result = result;
        }
    }
}
