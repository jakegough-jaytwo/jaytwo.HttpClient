using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication.Token
{
    public class DelegateTokenProvider : ITokenProvider
    {
        private readonly Func<string> _tokenDelegate;

        public DelegateTokenProvider(Func<string> tokenDelegate)
        {
            _tokenDelegate = tokenDelegate;
        }

        public string GetToken() => _tokenDelegate.Invoke();
    }
}
