using AzureB2C.Utility.Code.Interfaces;
using AzureB2C.Utility.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace AzureB2C.Utility.Code
{
    public class B2CUtility: IAzureB2CUtility
    {
        /// <summary>
        /// This key is only being used to encrypt/decrypt user identifiers, which are simply unique identifiers and not incredibly important for security.  For that reason I'm just adding a static generated salt for the key.
        /// </summary>
        private string SaltKey
        {
            get
            {
                return "Bod4m8WnlCTVMlY5oDc3xqxkc8oIaxAq";
            }
        }
        private AzureAuthenticationProvider Provider { get; set; }

        private string Authority { get; set; }
        private string Endpoint { get; set; }
        private string ClientTenant { get; set; }
        private string GraphVersion { get; set; }
        private string Resource { get; set; }
        private string APIClientId { get; set; }
        private string ApplicationId { get; set; }

         /// <summary>
        /// 
        /// </summary>
        /// <param name="clientTenant">e.g. {tenantname}.onmicrosoft.com</param>
        /// <param name="apiClientId">e.g. 7843408B-2222-4444-BBBB-9999670B9002</param>
        /// <param name="clientSecret">e.g. cDVhHRgB34HYJdGmeuitejRe68fdXGJ88EewGJpG1</param>
        /// <param name="applicationId">This is not required, but if it is not set the system will need to poll B2C for the Application ID.  e.g. 7843408B-2222-4444-BBBB-9999670B9002</param>
        /// <param name="authority"></param>
        /// <param name="endpoint"></param>
        /// <param name="graphVersion"></param>
        public B2CUtility(string clientTenant, string apiClientId, string clientSecret, string applicationId = "", string authority = "https://login.microsoftonline.com/", string endpoint = "https://graph.windows.net/", string graphVersion = "api-version=1.6", string resource = "https://graph.microsoft.com/")
        {
            Endpoint = endpoint;
            ClientTenant = clientTenant;
            GraphVersion = graphVersion;
            Authority = authority;
            Resource = resource;
            APIClientId = apiClientId;

            ApplicationId = applicationId;

            Provider = new AzureAuthenticationProvider(clientTenant, apiClientId, clientSecret, authority, endpoint);
        }

        #region Public User functions

        public string EncryptUserIdentifier(string userIdentifier)
        {
            var crypto = new Encryption();

            return crypto.Encrypt(SaltKey, userIdentifier);
        }

        public string DecryptUserIdentifier(string encryptedUserIdentifier)
        {
            var crypto = new Encryption();

            return crypto.Decrypt(SaltKey, encryptedUserIdentifier);
        }

        public async Task ActivateUserByEncryptedIdentifier(string encryptedUserIdentifier)
        {
            var decryptedUserIdentifier = DecryptUserIdentifier(encryptedUserIdentifier);

            await SetUserAccountStatus(decryptedUserIdentifier, true);
        }

        /// <summary>
        /// Creates the supplied user within B2C.
        /// Note: Upon initial creation, the supplied B2CUser object's AccountEnabled property should be set to false for best practices.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<B2CUser> CreateUser(B2CUser user)
        {
            if (user.SignInName == null)
            {
                user.SignInName = new SignInName();
            }

            user.ValidateUser();
            user.SignInName.ValidateCredentials();

            //// If the Extended Properties have not been set yet, then either load or generate them
            //if (ExtendedPropertyNames == null ||
            //    string.IsNullOrEmpty(ExtendedPropertyNames.SignatureLogoPath) || string.IsNullOrEmpty(ExtendedPropertyNames.SignaturePhotoPath) || string.IsNullOrEmpty(ExtendedPropertyNames.SignatureText) || string.IsNullOrEmpty(ExtendedPropertyNames.Consent))
            //{
            //    ExtendedPropertyNames = await GetExtendedPropertyNames();
            //}

            var newUser =
                new
                {
                    accountEnabled = user.AccountEnabled,
                    creationType = "LocalAccount",
                    signInNames = new object[]{
                        new
                        {
                            type = "emailAddress",
                            value = user.SignInName.Email
                        } },
                    //userPrincipalName = user.Email,
                    mailNickname = $"{user.GivenName.Replace(" ", "_").Replace(".", "")}{user.Surname.Replace(" ", "_").Replace(".", "")}",
                    displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? $"{user.GivenName} {user.Surname}" : user.DisplayName,
                    givenName = user.GivenName,
                    surname = user.Surname,
                    preferredLanguage = user.Language,
                    country = user.Country ?? "",
                    state = user.Province ?? "",
                    city = user.City ?? "",
                    streetAddress = user.StreetAddress ?? "",
                    postalCode = user.PostalCode,
                    telephoneNumber = user.Phone,
                    facsimileTelephoneNumber = user.Fax,
                    //companyName = user.Company,
                    jobTitle = user.JobTitle,
                    passwordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
                    passwordProfile =
                        new
                        {
                            password = user.SignInName.Password,
                            forceChangePasswordNextLogin = false
                        },
                };

            var jsonUser = JObject.FromObject(newUser);

            //var jsonUserString = jsonUser.ToString();

            //jsonUserString = jsonUserString.Replace("{{", "{");
            //jsonUserString = jsonUserString.Replace("}}", "}");
            //jsonUserString = jsonUserString.Replace("\r\n", "");

            var response = await SendGraphRequest(HttpMethod.Post, GraphApi.UsersGeneric, null, jsonUser, null);
            var createdUser = new JavaScriptSerializer().Deserialize<GraphUser>(response);

            user.UserId = createdUser.objectId;

            return user;
        }

        public async Task<B2CUser> UpdateUser(B2CUser user)
        {
            user.ValidateUser();


            var newUser =
                   new
                   {
                       mailNickname = $"{user.GivenName.Replace(" ", "_").Replace(".", "")}{user.Surname.Replace(" ", "_").Replace(".", "")}",
                       displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? $"{user.GivenName} {user.Surname}" : user.DisplayName,
                       givenName = user.GivenName,
                       surname = user.Surname,
                       preferredLanguage = user.Language,
                       country = user.Country,
                       state = user.Province,
                       city = user.City,
                       streetAddress = user.StreetAddress,
                       postalCode = user.PostalCode,
                       telephoneNumber = user.Phone,
                       facsimileTelephoneNumber = user.Fax,
                       //companyName = user.Company,
                       jobTitle = user.JobTitle,
                   };

            var jsonUser = JObject.FromObject(newUser);


            //var jsonUserString = jsonUser.ToString();

            //jsonUserString = jsonUserString.Replace("{{", "{");
            //jsonUserString = jsonUserString.Replace("}}", "}");

            var response = await SendGraphRequest(new HttpMethod("PATCH"), GraphApi.UserSpecific, user.UserId.ToString(), jsonUser, null);
            var updatedUser = new JavaScriptSerializer().Deserialize<GraphUser>(response);

            return user;
        }

        public async Task UpdateUserPassword(string userIdentifier, string password)
        {
            var newUser =
                   new
                   {
                       passwordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
                       passwordProfile =
                        new
                        {
                            password = password,
                            forceChangePasswordNextLogin = false
                        }
                   };

            var jsonUser = JObject.FromObject(newUser);

            await SendGraphRequest(new HttpMethod("PATCH"), GraphApi.UserSpecific, userIdentifier, jsonUser, null);
        }

        /// <summary>
        /// Note: This method will not work due to B2C restrictions.
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        private async Task<string> ChangeUserPassword(string userIdentifier, string currentPassword, string newPassword)
        {
            var newUser =
                   new
                   {
                       currentPassword = currentPassword,
                       newPassword = newPassword
                   };

            var jsonUser = JObject.FromObject(newUser);

            return await SendGraphRequest(HttpMethod.Post, GraphApi.UserChangePassword, userIdentifier, jsonUser, null);
        }

        /// <summary>
        /// Enabled or disables a user's B2C account
        /// </summary>
        /// <param name="userIdentifier">Accepts either a user's Guid ID or a User Principal Name</param>
        /// <param name="accountEnabled">Enabled/Disabled</param>
        /// <returns></returns>
        public async Task SetUserAccountStatus(string userIdentifier, bool accountEnabled)
        {
            var json = JObject.FromObject(new { accountEnabled = accountEnabled });

            var response = await SendGraphRequest(new HttpMethod("PATCH"), GraphApi.UserSpecific, userIdentifier, json, null);
        }

        /// <summary>
        /// Gets a B2C user
        /// </summary>
        /// <param name="userIdentifier">Accepts either a user's Guid ID or a User Principal Name</param>
        /// <returns></returns>
        public B2CUser GetB2CUser(string userIdentifier)
        {
            var response = Task.Run(() => GetUserByIdentifier(userIdentifier)).Result;
            var jsonUser = JObject.Parse(response);

            var user = GetUserByResponseObject(jsonUser);

            return user;
        }

        public async Task<B2CUser> GetB2CUserByEmail(string email)
        {

            var filter = $"$filter=signInNames/any(x:x/value eq '{HttpUtility.UrlEncode(email)}')";

            var response = await SendGraphRequest(HttpMethod.Get, GraphApi.UsersGeneric, null, null, filter);

            var jsonResponse = JObject.Parse(response);

            // Since the filter is designed to return a list we need to parse out the user object.  Don't worry, we should only ever get 1 user at a time from this.
            if (jsonResponse.Last.Last.Count() == 0)
            {
                return null;
            }

            var jsonUser = JObject.FromObject(jsonResponse.Last.Last[0]);

            var user = GetUserByResponseObject(jsonUser);

            return user;
        }

        #endregion

        #region Private User functions

        private B2CUser GetUserByResponseObject(JObject userObject)
        {
            var userEmail = "";
            foreach (var signInName in userObject["signInNames"])
            {
                if ((signInName["type"].ToString() ?? "") == "emailAddress")
                {
                    userEmail = signInName["value"].ToString() ?? "";
                }
            }

            var user = new B2CUser()
            {
                SignInName = new SignInName()
                {
                    Email = userEmail
                },
                UserId = new Guid((userObject["objectId"] ?? "").ToString()),
                PrincipalName = (userObject["userPrincipalName"] ?? "").ToString(),
                GivenName = (userObject["givenName"] ?? "").ToString(),
                Surname = (userObject["surname"] ?? "").ToString(),
                DisplayName = (userObject["displayName"] ?? "").ToString(),
                Country = (userObject["country"] ?? "").ToString(),
                Province = (userObject["province"] ?? "").ToString(),
                City = (userObject["city"] ?? "").ToString(),
                StreetAddress = (userObject["streetAddress"] ?? "").ToString(),
                PostalCode = (userObject["postalCode"] ?? "").ToString(),
                Company = (userObject["company"] ?? "").ToString(),
                JobTitle = (userObject["jobTitle"] ?? "").ToString(),
                Phone = (userObject["telephoneNumber"] ?? "").ToString(),
                Language = (userObject["preferredLanguage"] ?? "").ToString(),
                Fax = (userObject["facsimileTelephoneNumber"] ?? "").ToString(),
            };

            return user;
        }

        private async Task<string> GetUserByIdentifier(string principalName)
        {
            return await SendGraphRequest(HttpMethod.Get, GraphApi.UserSpecific, principalName);
            //return await SendGraphRequest(HttpMethod.Get, GraphApi.UsersGeneric, null, null, $"$filter=signInNames/any(x:x/value eq '{email}')");
        }

        private async Task<string> GetUsers()
        {
            return await SendGraphRequest(HttpMethod.Get, GraphApi.UsersGeneric, null, null, null);
        }

        public async Task DeleteUser(string userIdentifier)
        {
            await SendGraphRequest(HttpMethod.Delete, GraphApi.UserSpecific, userIdentifier, null, null);
        }

        #endregion

        #region Extended Properties

        public async Task<Guid> GetApplicationId()
        {
            var appId = await GetApplicationIdString();

            return new Guid(appId);
        }

        private async Task<string> GetApplicationIdString()
        {
            var cache = MemoryCache.Default;
            var applicationId = cache["B2C_ApplicationId"] as string;

            // If we already have the ID cached, just immediately return it instead of retrieving it from the server
            if (!string.IsNullOrWhiteSpace(applicationId))
            {
                return applicationId;
            }

            AuthenticationResult authenticationResult = Provider.AuthenticationResult;
            HttpClient http = new HttpClient();
            string url = $"{Endpoint}{ClientTenant}/applications?{GraphVersion}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            var applicationsResponse = await response.Content.ReadAsStringAsync();

            JObject applications = JObject.Parse(applicationsResponse);

            foreach (var application in applications["value"].ToArray())
            {
                if (application["appId"] != null && application["appId"].ToString() == APIClientId)
                {
                    if (application["objectId"] != null && !string.IsNullOrWhiteSpace(application["objectId"].ToString()))
                    {
                        if (string.IsNullOrWhiteSpace(applicationId))
                        {
                            applicationId = application["objectId"].ToString();
                            cache.Set("B2C_ApplicationId", applicationId, new CacheItemPolicy() { });
                        }

                        return applicationId;
                    }
                }
            }

            throw new Exception("Could not retrieve Application ID for the supplied Client ID.");
        }

        #endregion

        #region Generic

        private async Task<string> SendGraphRequest(HttpMethod httpMethod, GraphApi graphApi, string userId = null, JObject json = null, string queryParameters = null)
        {
            string api;
            switch (graphApi)
            {
                case GraphApi.Me:
                    api = "/me";
                    break;
                case GraphApi.UsersGeneric:
                    api = "/users";
                    break;
                case GraphApi.UserSpecific:
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        throw new ArgumentNullException("userId", "No user ID supplied.");
                    }

                    api = $"/users/{userId}";
                    break;
                case GraphApi.UserChangePassword:
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        throw new ArgumentNullException("userId", "No user ID supplied.");
                    }

                    api = $"/users/{userId}/changePassword";
                    break;
                default:
                    throw new NotImplementedException("Selected Graph API has not been implemented.");
            }

            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult authenticationResult = Provider.AuthenticationResult;
            HttpClient http = new HttpClient();
            string url = $"{Endpoint}{ClientTenant}{api}?{GraphVersion}";

            // If there are query parameters, add them
            if (!string.IsNullOrEmpty(queryParameters))
            {
                url += "&" + queryParameters;
            }

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            if (json != null)
            {
                var jsonString = json.ToString();
                request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        #region Logged-in User functions

        public async Task<string> GetCurrentB2CUser(string idToken)
        {
            return await SendGraphRequest(idToken);
        }

        private async Task<string> SendGraphRequest(string idTokenForUser)
        {
            string api = "/me";

            AuthenticationResult authenticationResult = Provider.AuthenticationResultForUser(idTokenForUser);
            HttpClient http = new HttpClient();
            string url = $"{Endpoint}{ClientTenant}{api}?{GraphVersion}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Gets a B2C user
        /// </summary>
        /// <param name="userIdentifier">Accepts either a user's Guid ID or a User Principal Name</param>
        /// <returns></returns>
            #endregion
    }
}
