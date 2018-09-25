using AzureB2C.Utility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureB2C.Utility.Code.Interfaces
{
    public interface IAzureB2CUtility
    {
        string EncryptUserIdentifier(string userIdentifier);
        string DecryptUserIdentifier(string encryptedUserIdentifier);
        Task ActivateUserByEncryptedIdentifier(string encryptedUserIdentifier);
        Task<B2CUser> CreateUser(B2CUser user);
        Task<B2CUser> UpdateUser(B2CUser user);
        Task UpdateUserPassword(string userIdentifier, string password);
        Task SetUserAccountStatus(string userIdentifier, bool accountEnabled);
        B2CUser GetB2CUser(string userIdentifier);
        Task<B2CUser> GetB2CUserByEmail(string email);
        Task DeleteUser(string userIdentifier);
        Task<Guid> GetApplicationId();
        Task<string> GetCurrentB2CUser(string idToken);
    }
}
