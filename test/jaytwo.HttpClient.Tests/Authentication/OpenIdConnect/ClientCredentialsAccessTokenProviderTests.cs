using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Authentication.Token.OpenIdConnect;
using jaytwo.MimeHelper;
using Moq;
using Xunit;

namespace jaytwo.HttpClient.Tests.Authentication.OpenIdConnect
{
    public class ClientCredentialsAccessTokenProviderTests
    {
        [Fact]
        public async Task GetAccessTokenAsync_works()
        {
            // arrange
            var config = new ClientCredentialsTokenConfig()
            {
                TokenUrl = "https://example.com/oidc/access_token",
            };

            var accessToken = "howdy";
            var expiresIn = 777;
            var tokenType = "banana";

            var mockResponse =
                $@"
{{
    ""access_token"": ""{accessToken}"",
    ""expires_in"": {expiresIn},
    ""token_type"": ""{tokenType}""
}}";

            var mockHttp = new Mock<IHttpClient>();
            mockHttp
                .Setup(x => x.SendAsync(It.IsAny<HttpRequest>()))
                .ReturnsAsync(new HttpResponse()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = mockResponse,
                    Headers = new Dictionary<string, string>()
                    {
                        { Constants.Headers.ContentType, MediaType.application_json },
                    },
                });

            var tokenProvider = new ClientCredentialsAccessTokenProvider(mockHttp.Object, config);

            // act
            var accessTokenResponse = await tokenProvider.GetAccessTokenAsync();

            // assert
            Assert.Equal(accessToken, accessTokenResponse.AccessToken);
            Assert.Equal(expiresIn, accessTokenResponse.ExpiresIn);
            Assert.Equal(tokenType, accessTokenResponse.TokenType);
        }
    }
}
