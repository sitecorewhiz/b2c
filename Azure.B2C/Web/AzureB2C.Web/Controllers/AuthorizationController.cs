using AzureB2C.Utility;
using AzureB2C.Utility.Code;
using AzureB2C.Utility.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureB2C2.Controllers
{
    public class AuthorizationController : Controller
    {
        public const string B2CUserSessionId = "B2C_User";
        public const string B2CClaimUserIdIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        private string ApplicationId = ConfigurationManager.AppSettings["ida:ApplicationId"];
        private string ADApiClientKey = ConfigurationManager.AppSettings["ida:ADApiClientKey"];
        private string ADApiClientId = ConfigurationManager.AppSettings["ida:ADApiClientId"];
        private string ClientTenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        private string PostSignInRedirectUri = ConfigurationManager.AppSettings["ida:PostSignInRedirectUri"];
        private string PostLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public Guid? B2CCUserId
        {
            get
            {
                var claimsPrincipal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claimsIdentity = ((ClaimsIdentity)claimsPrincipal.Identity);
                var userIdClaim = claimsIdentity.FindFirst(B2CClaimUserIdIdentifier);
                var userId = userIdClaim?.Value;

                // If a valid user ID has been retrieved from the claim, return it as a guid
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return new Guid(userId);
                }

                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public B2CUser B2CUser
        {
            get
            {
                // If the user's claim ID is no longer available, clear the user object from the session
                if (!B2CCUserId.HasValue || B2CCUserId.Value == Guid.Empty)
                {
                    Session[B2CUserSessionId] = null;
                }

                // If the user's claim ID is available but the user's session object is null, try to populate the user's session object
                if (B2CCUserId.HasValue && B2CCUserId.Value != Guid.Empty && Session[B2CUserSessionId] == null)
                {
                    try
                    {
                        Session[B2CUserSessionId] = Task.Run(() => B2CUtility.GetB2CUser(B2CCUserId.Value.ToString())).Result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not retrieve the user for the supplied user ID.", ex);
                    }
                }

                return (B2CUser)Session[B2CUserSessionId];
            }
        }

        private B2CUtility B2CUtility { get; set; }

        public AuthorizationController()
        {
            B2CUtility = new B2CUtility(ClientTenant, ADApiClientId, ADApiClientKey, ApplicationId);
        }

        [Authorize]
        public void SignIn()
        {
            // This method no longer needs code!  
            //There are apparently issues with using OWIN challenge alongside Sitecore, but decorating a method with [Authorize] will trigger the Owin challenge properly and not interfere with Sitecore's use of Owin.

            HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = PostSignInRedirectUri }, SignInPolicyId);
        }

        public RedirectResult SignOut(string callbackUrl = null)
        {
            callbackUrl = callbackUrl ?? PostLogoutRedirectUri;

            // Sign the user out via OWIN
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);


            // Remove the user's ID from claims
            var claimsPrincipal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var claimsIdentity = ((ClaimsIdentity)claimsPrincipal.Identity);
            var userIdClaim = claimsIdentity.FindFirst(B2CClaimUserIdIdentifier);
            claimsIdentity.RemoveClaim(userIdClaim);

            // this block only exists because I've seen situations where OWIN doesn't immediately clear the claims on sign-out

            // Clear the user's data from session
            Session[B2CUserSessionId] = null;

            // Redirect the user to callback page
            return Redirect(callbackUrl);
        }

        /// <summary>
        /// Generates a URL that will direct a user to a method which activates their account
        /// </summary>
        /// <param name="userIdentifier">UserId</param>
        /// <returns></returns>
        public string GenerateUserActivationUrl(string userIdentifier)
        {
            // This may or may not need to be modified slightly
            string activationUrl = Url.Action("ActivateUser", "Authorization", new { authenticationToken = HttpUtility.UrlEncode(B2CUtility.EncryptUserIdentifier(userIdentifier)) });

            return activationUrl;
        }

        /// <summary>
        /// Activates a user by their ancyrpted user identifier
        /// </summary>
        /// <param name="authenticationToken">The encrypted user identifier</param>
        public async void ActivateUser(string authenticationToken)
        {
            // Activate the user with Graph API
            await B2CUtility.ActivateUserByEncryptedIdentifier(HttpUtility.UrlDecode(authenticationToken));

            // Redirect user to sign-in page
            SignIn();
        }
    }
}