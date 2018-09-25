**************************************************************

Readme for the Azure B2C implementation


**************************************************************


1. {tenant_name} - {string} - The tenant name of the Azure B2C tenant. This will be of the format (without braces) - "custom_tenant_name.onmicrosoft.com".
2. {graph_api_app_registrations_id} - {GUID} - The id which is registered after registering the B2C app in the app registrations section of the B2C tenant.
3. {graph_api_one_time_key} - One time key which is generated in the portal.azure.com when registering the B2C app in the app registrations section of the B2C tenant.
4. {b2c_tenant_id} - {GUID} - The guid of the b2c tenant created in the portal.azure.com
5. {SignIn_Policy_Name} - {string} - The signin policy name created in the b2c tenant.
6. {ForgotPassword_Policy_Name} - {string} - The forgot password policy name created in the b2c tenant.
7. {PostSignIn_Redirect_URI} - {uri} - URI which will be specificed in the Owin startup class for redirection after successful sign in.
8. {PostLogout_Redirect_URI} - {uri} - URI which will be specified in the Owin startup class for redirection after successful logout.
