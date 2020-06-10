using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication.Token.OpenIdConnect
{
    internal interface IAccessTokenProvider
    {
        Task<AccessTokenResponse> GetAccessTokenAsync();
    }
}
