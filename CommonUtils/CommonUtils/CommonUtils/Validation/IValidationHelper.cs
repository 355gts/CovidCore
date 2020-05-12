namespace CommonUtils.Validation
{
    public interface IValidationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToValidate"></param>
        /// <exception cref=""></exception>
        void Validate<T>(T objectToValidate);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToValidate"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        bool TryValidate<T>(T objectToValidate, out string errorMessage);
    }
}
