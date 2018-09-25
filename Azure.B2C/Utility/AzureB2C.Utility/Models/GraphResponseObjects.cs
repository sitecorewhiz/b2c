using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureB2C.Utility.Models
{
    public class GraphResponse
    {
        public bool Success { get; set; }
        public GraphUser Result { get; set; }
    }

    public class GraphProperty
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class GraphUser
    {
        public Guid objectId { get; set; } // UserId
        public string deletionTimestamp { get; set; }
        public bool accountEnabled { get; set; }
        public string city { get; set; }
        public string companyName { get; set; }
        public string country { get; set; }
        public string creationType { get; set; }
        public string displayName { get; set; }
        public string facsimileTelephoneNumber { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mailNickname { get; set; }
        public string mobile { get; set; }
        public string postalCode { get; set; }
        public string preferredLanguage { get; set; }
        public GraphProperty[] signInNames { get; set; }
        public string state { get; set; }
        public string streetAddress { get; set; }
        public string surname { get; set; }
        public string telephoneNumber { get; set; }
        public string userPrincipalName { get; set; }
        public string userType { get; set; }
    }
}