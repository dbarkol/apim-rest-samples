using Azure.Core;
using Azure.Identity;
using System.Threading.Tasks;

namespace Samples
{
    internal static class Utils
    {
        internal static async Task<string> GetAccessTokenWithManagedIdentity()
        {
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var credential = new ChainedTokenCredential(new AzureCliCredential(), new ManagedIdentityCredential());
            AccessToken token = await credential.GetTokenAsync(tokenRequestContext);
            return token.Token;
        }
    }

}