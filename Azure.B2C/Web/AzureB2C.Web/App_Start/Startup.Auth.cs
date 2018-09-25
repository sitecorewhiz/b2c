using Owin;
using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Configuration;
using Microsoft.Owin;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(AzureB2C.Startup))]
namespace AzureB2C
{
    public class Startup
    {
        private static string AadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        private static string B2CApiClientId = ConfigurationManager.AppSettings["ida:B2CApiClientId"];
        private static string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        private static string PostSignInRedirectUri = ConfigurationManager.AppSettings["ida:PostSignInRedirectUri"];
        private static string PostLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // Generate the metadata address using the tenant and policy information
                    MetadataAddress = String.Format(AadInstance, Tenant, SignInPolicyId),

                    AuthenticationType = SignInPolicyId,

                    // These are standard OpenID Connect parameters, with values pulled from web.config
                    ClientId = B2CApiClientId,
                    RedirectUri = PostSignInRedirectUri,
                    PostLogoutRedirectUri = PostLogoutRedirectUri,

                    // Specify the callbacks for each type of notifications
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        //RedirectToIdentityProvider = OnRedirectToIdentityProvider,
                        //AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        //AuthenticationFailed = OnAuthenticationFailed,
                    },

                    // Specify the claims to validate
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name"
                    },
                    ResponseType = "id_token",
                    Scope = "openid"
                }
            );
        }
    }
}