using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AADAppSample
{
    public class ClientSecretAuthProvider : IAuthenticationProvider
    {
        private IConfidentialClientApplication _msalClient;
        private string _appId;
        private string _clientSecret;
        private string[] _scopes;
        private string _tenantId;

        public ClientSecretAuthProvider(string appId, string[] scopes, string tenantId, string clientSecret)
        {
            _appId = appId;
            _clientSecret = clientSecret;
            _scopes = scopes;
            _tenantId = tenantId;
            _msalClient = ConfidentialClientApplicationBuilder.Create(this._appId)
                .WithAuthority(AzureCloudInstance.AzurePublic, this._tenantId)
                .WithClientSecret(this._clientSecret)
                .Build();
        }

        public async Task<string> GetAuthorizationHeader()
        {
            try
            {
                // AcquireTokenForClient (Client credentials flow),
                // which does not use the user token cache,
                // but an application token cache. This method                 
                // care of verifying this application token cache
                // before sending a request to the STS
                var result = await this._msalClient
                    .AcquireTokenForClient(this._scopes)
                    .ExecuteAsync();
                return result.CreateAuthorizationHeader();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error getting access token: {exception.Message}");
                return null;
            }
        }


        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Add("Authorization", await GetAuthorizationHeader());
        }
    }
}