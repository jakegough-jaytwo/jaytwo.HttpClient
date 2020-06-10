using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication.Token.OpenIdConnect
{
    internal class ClientCredentialsAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly ClientCredentialsTokenConfig _config;

        public ClientCredentialsAccessTokenProvider(IHttpClient httpClient, ClientCredentialsTokenConfig config)
        {
            _httpClient = httpClient ?? HttpClient.Default;
            _config = config;
        }

        public async Task<AccessTokenResponse> GetAccessTokenAsync()
        {
            var payload = new
            {
                grant_type = "client_credentials",
                client_id = _config.ClientId,
                client_secret = _config.ClientSecret,
                resource = _config.Resource,
            };

            var httpResponse = await _httpClient.SendAsync(httpRequest =>
            {
                httpRequest
                    .WithMethod(HttpMethod.Post)
                    .WithUri(_config.TokenUrl)
                    .WithJsonContent(payload);
            });

            var result = httpResponse.As<AccessTokenResponse>();
            return result;
        }
    }
}
