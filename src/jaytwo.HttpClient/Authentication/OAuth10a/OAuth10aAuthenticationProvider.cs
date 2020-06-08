#if !NETSTANDARD1_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.DateTimeHelper;
using jaytwo.HttpClient.Constants;
using jaytwo.MimeHelper;
using jaytwo.UrlHelper;

namespace jaytwo.HttpClient.Authentication.OAuth10a
{
    public class OAuth10aAuthenticationProvider : AuthenticationProviderBase, IAuthenticationProvider
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _token;
        private readonly string _tokenSecret;

        public OAuth10aAuthenticationProvider(string consumerKey, string consumerSecret)
            : this(consumerKey, consumerSecret, null, null)
        {
        }

        public OAuth10aAuthenticationProvider(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public string OauthVersion { get; set; } = "1.0";

        internal Func<DateTimeOffset> GetUtcNowDelegate { get; set; } = () => DateTimeOffset.UtcNow;

        internal Func<string> GetNonceDelegate { get; set; } = () => Guid.NewGuid().ToString();

        public override void Authenticate(HttpRequest request)
        {
            var calculator = new OAuth10aSignatureCalculator()
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Token = _token,
                TokenSecret = _tokenSecret,
                HttpMethod = request.Method,
                Url = request.Uri.AbsoluteUri,
                Timestamp = GetUtcNowDelegate.Invoke().ToUnixTimeSeconds(),
                Nonce = GetNonceDelegate.Invoke(),
                OauthVersion = OauthVersion,
            };

            if (request.ContentType == MediaType.application_x_www_form_urlencoded)
            {
                calculator.BodyParameters = QueryString.Deserialize(request.Content).ToDictionary(x => x.Key, x => x.Value.First()); // TODO: care about the edge case where the URL may contain multiple parameters of the same name
            }

            var authorizationHeaderValue = calculator.GetAuthorizationHeaderValue();
            request.Headers[Headers.Authorization] = authorizationHeaderValue;
        }
    }
}

#endif
