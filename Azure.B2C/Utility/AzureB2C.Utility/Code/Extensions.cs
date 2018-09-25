using AzureB2C.Utility.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureB2C.Utility.Code
{
    public static class B2CExtensions
    {
        public static bool ValidateUser(this B2CUser user)
        {
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, new ValidationContext(user), validationResults);

            if (!isValid)
            {
                throw new B2CUserException("The supplied user is invalid.", validationResults);
            }

            return isValid;
        }

        public static bool ValidateCredentials(this SignInName credentials)
        {
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(credentials, new ValidationContext(credentials), validationResults);

            if (!isValid)
            {
                throw new B2CUserException("The supplied credentials are invalid.", validationResults);
            }

            return isValid;
        }
    }
}
