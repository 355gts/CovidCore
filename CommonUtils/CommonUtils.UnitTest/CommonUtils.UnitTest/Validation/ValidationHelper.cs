using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using V = CommonUtils.Validation;

namespace CommonUtils.Test.Validation
{
    [TestClass]
    public class ValidationHelper
    {

        [TestClass]
        public class Validate
        {

            [TestMethod]
            public void NotValid()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "All these fields are invalid!",
                    Number = 10000000,
                };
                
                bool exceptionThrown = false;

                try
                {
                    validationHelper.Validate(obj);
                }
                catch (ValidationException vEx)
                {
                    exceptionThrown = true;
                    Assert.IsFalse(String.IsNullOrWhiteSpace(vEx.Message));
                    Assert.IsTrue(vEx.Message.Contains("maximum"));    // Name
                    Assert.IsTrue(vEx.Message.Contains("between"));    // Number
                    Assert.IsTrue(vEx.Message.Contains("required"));   // Other
                }

                Assert.IsTrue(exceptionThrown);
            }
            
            [TestMethod]
            public void AllValid()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "ThisIsValid",
                    Number = 777,
                    Other = "Present"
                };

                string errorMessage = String.Empty;

                bool exceptionThrown = false;

                try
                {
                    validationHelper.Validate(obj);
                }
                catch (ValidationException)
                {
                    exceptionThrown = true;
                }

                Assert.IsFalse(exceptionThrown);
            }
        }

        [TestClass]
        public class TryValidate
        {

            [TestMethod]
            public void MissingRequiredField()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "OtherMissing",
                    Number = 10
                };
                
                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("required"));
            }

            [TestMethod]
            public void StringLengthTooLong()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "ThisIsJustAboutOver20",
                    Number = 21,
                    Other = "Present"
                };

                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("maximum"));
            }

            [TestMethod]
            public void StringLengthTooShort()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "min",
                    Number = 3,
                    Other = "Present"
                };
                
                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("minimum"));
            }

            [TestMethod]
            public void NumberTooLarge()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "BigNumber",
                    Number = 1000,
                    Other = "Present"
                };

                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("between"));
            }

            [TestMethod]
            public void NumberTooSmall()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "SmallNumber",
                    Number = 1,
                    Other = "Present"
                };

                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("between"));
            }

            [TestMethod]
            public void NumberIsNegative()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "NegativeNumber",
                    Number = -777,
                    Other = "Present"
                };

                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(obj, out errorMessage));

                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
                Assert.IsTrue(errorMessage.Contains("between"));
            }
            
            [TestMethod]
            public void AllValid()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                ValidationObj obj = new ValidationObj
                {
                    Name = "ThisIsValid",
                    Number = 777,
                    Other = "Present"
                };
                
                string errorMessage = String.Empty;

                Assert.IsTrue(validationHelper.TryValidate(obj, out errorMessage));
                Assert.IsTrue(String.IsNullOrWhiteSpace(errorMessage));
            }

            [TestMethod]
            public void ValidateList()
            {
                V.IValidationHelper validationHelper = new V.ValidationHelper();

                var validationObjects = new List<ValidationObj>()
               {
                    new ValidationObj
                    {
                        Name = "ThisIsValid",
                        Number = 777,
                        Other = "Present"
                    },
                    new ValidationObj
                    {
                        Name = "ThisIsNot",
                        Number = 0,
                    },
               };
               
                string errorMessage = String.Empty;

                Assert.IsFalse(validationHelper.TryValidate(validationObjects, out errorMessage));
                Assert.IsFalse(String.IsNullOrWhiteSpace(errorMessage));
            }
        }
    }

    public class ValidationObj
    {
        [StringLength(20, MinimumLength = 5)]
        public string Name { get; set; }

        [Range(9, 999)]
        public int Number { get; set; }

        [Required]
        public string Other { get; set; }
    }
}
