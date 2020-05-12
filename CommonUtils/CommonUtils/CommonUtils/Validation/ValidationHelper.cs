using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CommonUtils.Validation
{
    public sealed class ValidationHelper : IValidationHelper
    {
        public bool TryValidate<T>(T objectToValidate, out string errorMessage)
        {
            errorMessage = String.Empty;
            bool success = false;
            var validationContext = new ValidationContext(objectToValidate);
            var validationResults = new List<ValidationResult>();

            if ((objectToValidate is string) || (objectToValidate is DateTime) || (objectToValidate is bool))
            {
                // don't bother validating above types
                success = true;
            }
            else
            {

                IEnumerable enumerable = objectToValidate as IEnumerable;

                if (enumerable == null)
                {
                    // simply validate the object
                    success = Validator.TryValidateObject(objectToValidate, validationContext, validationResults, true);
                    errorMessage = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage).ToArray());
                }
                else
                {
                    // assume success until we hit a invalid item
                    success = true;
                    List<string> tempErrors = new List<string>();
                    string tempError;

                    // enumerate and validate each object
                    foreach (var item in enumerable)
                    {
                        if (!TryValidate(item, out tempError))
                        {
                            success = false;
                            tempErrors.Add(tempError);
                        }
                    }
                    errorMessage = string.Join("; ", tempErrors);
                }
            }

            return success;
        }

        public void Validate<T>(T objectToValidate)
        {
            string errorMessage = string.Empty;
            bool success = TryValidate(objectToValidate, out errorMessage);

            if (!success)
                throw new ValidationException(errorMessage);
        }
    }
}
