using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzureB2C.Utility.Models
{
    public class B2CUser
    {
        public Guid UserId { get; set; }

        public SignInName SignInName { get; set; }

        /// <summary>
        /// B2C value cannot be overriden
        /// </summary>
        public string PrincipalName { get; set; }
        public string DisplayName { get; set; }

        /// <summary>
        /// The user account has been enabled
        /// </summary>
        public bool AccountEnabled { get; set; }

        public string Language { get; set; }

        /* Personal info */
        [Required]
        public string GivenName { get; set; }
        [Required]
        public string Surname { get; set; }

        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string Country { get; set; } // Country or Region
        public string Province { get; set; } // Province or State
        public string City { get; set; }
        public string PostalCode { get; set; } // Postal or ZIP
        public string StreetAddress { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }
    public class SignInName
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
