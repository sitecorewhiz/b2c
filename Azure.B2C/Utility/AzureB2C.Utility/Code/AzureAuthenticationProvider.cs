using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;

namespace AzureB2C.Utility.Code
{
    public class AzureAuthenticationProvider
    {
        private string Authority { get; set; }
        private string Endpoint { get; set; }
        private string ClientTenant { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="endpoint"></param>
        /// <param name="clientTenant"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public AzureAuthenticationProvider(string clientTenant, string clientId, string clientSecret, string authority = "https://login.microsoftonline.com/", string endpoint = "https://graph.windows.net/")
        {
            ClientTenant = clientTenant;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            Endpoint = endpoint;
        }

        private AuthenticationContext authenticationContext;
        public AuthenticationContext AuthenticationContext
        {
            get
            {
                if (authenticationContext == null)
                    authenticationContext = new AuthenticationContext($"{Authority}{ClientTenant}");

                return authenticationContext;
            }
        }

        private ClientCredential clientCredential;
        public ClientCredential ClientCredential
        {
            get
            {
                if (clientCredential == null)
                    clientCredential = new ClientCredential(ClientId, ClientSecret);

                return clientCredential;
            }
        }

        public AuthenticationResult AuthenticationResult
        {
            get
            {
                return AuthenticationContext.AcquireTokenAsync(Endpoint, ClientCredential).Result;
            }
        }

        public AuthenticationResult AuthenticationResultForUser(string idToken)
        {
            UserAssertion userAssertion = new UserAssertion(idToken);
            return AuthenticationContext.AcquireTokenAsync(Endpoint, ClientCredential, userAssertion).Result;
        }

        public Task<AuthenticationResult> UserAuthenticationResult(string username, string password)
        {
            //new UserPasswordCredential(username, password)
            return AuthenticationContext.AcquireTokenAsync(Endpoint, ClientCredential, new UserAssertion(AuthenticationResult.AccessToken, "urn:ietf:wg:oauth:2.0:oob", username));
        }
    }
}
