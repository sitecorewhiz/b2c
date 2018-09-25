using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AzureB2C.Utility.Models
{
    public class B2CUserException : Exception
    {
        public List<ValidationResult> ValidationErrors { get; set; }

        public B2CUserException(List<ValidationResult> validationErrors) : base()
        {
            ValidationErrors = validationErrors;
        }

        public B2CUserException(string message, List<ValidationResult> validationErrors) : base(message)
        {
            ValidationErrors = validationErrors;
        }
    }
}
