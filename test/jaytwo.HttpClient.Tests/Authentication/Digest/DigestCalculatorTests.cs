using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Unicode;
using jaytwo.HttpClient.Authentication.Digest;
using Xunit;

namespace jaytwo.HttpClient.Tests.Authentication.Digest
{
    public class DigestCalculatorTests
    {
        [Fact]
        public void GetHA1()
        {
            // arrange
            var algorithm = "MD5";
            var realm = "testrealm@host.com";
            var nonce = "dcd98b7102dd2f0e8b11d0f600bfb0c093";
            var user = "Mufasa";
            var pass = "Circle Of Life";
            var clientNonce = "0a4f113b";

            // act
            var ha1 = DigestCalculator.GetHA1(algorithm, realm, nonce, user, pass, clientNonce);

            // assert
            Assert.Equal("939e7578ed9e3c518a452acee763bce9", ha1);
        }

        [Fact]
        public void GetHA2()
        {
            // arrange
            var qop = "auth";
            var url = "/dir/index.html";
            var method = "GET";

            // act
            var ha1 = DigestCalculator.GetHA2(qop, url, method, null);

            // assert
            Assert.Equal("39aff3a2bab6126f332b942af96d3366", ha1);
        }

        [Fact]
        public void GetHA2_auth_int_binary_content()
        {
            // arrange
            var qop = "auth-int";
            var url = "/dir/index.html";
            var method = "GET";

            var httpRequest = new HttpRequest()
            {
                ContentBytes = Encoding.UTF8.GetBytes("Hello World"),
            };

            // act
            var ha1 = DigestCalculator.GetHA2(qop, url, method, httpRequest);

            // assert
            Assert.Equal("ce4bada06e032f3a3fd11b2eee60c8e8", ha1);
        }

        [Fact]
        public void GetHA2_auth_int_string_content()
        {
            // arrange
            var qop = "auth-int";
            var url = "/dir/index.html";
            var method = "GET";

            var httpRequest = new HttpRequest()
            {
                Content = "Hello World",
            };

            // act
            var ha1 = DigestCalculator.GetHA2(qop, url, method, httpRequest);

            // assert
            Assert.Equal("ce4bada06e032f3a3fd11b2eee60c8e8", ha1);
        }

        [Fact]
        public void GetResponse()
        {
            // arrange
            var qop = "auth";
            var nonce = "dcd98b7102dd2f0e8b11d0f600bfb0c093";
            var ha1 = "939e7578ed9e3c518a452acee763bce9";
            var ha2 = "39aff3a2bab6126f332b942af96d3366";
            var nonceCount = "00000001";
            var clientNonce = "0a4f113b";

            // act
            var response = DigestCalculator.GetResponse(qop, nonce, ha1, ha2, clientNonce, nonceCount);

            // assert
            Assert.Equal("6629fae49393a05397450978507c4ef1", response);
        }

        [Fact]
        public void GetResponse_From_DigestServerParams_and_HttpRequest()
        {
            // arrange
            var digestServerParams = new DigestServerParams()
            {
                Algorithm = "MD5",
                Nonce = "dcd98b7102dd2f0e8b11d0f600bfb0c093",
                Qop = "auth",
                Realm = "testrealm@host.com",
            };

            var request = new HttpRequest()
            {
                Method = HttpMethod.Get,
                Uri = new Uri("http://www.example.com/dir/index.html"),
            };

            var uri = request.Uri.PathAndQuery;
            var user = "Mufasa";
            var pass = "Circle Of Life";
            var nonceCount = "00000001";
            var clientNonce = "0a4f113b";

            // act
            var response = DigestCalculator.GetResponse(digestServerParams, request, uri, user, pass, clientNonce, nonceCount);

            // assert
            Assert.Equal("6629fae49393a05397450978507c4ef1", response);
        }

        [Theory]
        [InlineData("auth", Qop.Auth)]
        [InlineData("auth,auth-int", Qop.Auth)]
        [InlineData("auth-int", Qop.AuthInt)]
        [InlineData("auth,auth-int,foo", Qop.Auth)]
        [InlineData("foo", "foo")]
        public void SelectQop(string qop, string expected)
        {
            // arrange

            // act
            var selectedQop = DigestCalculator.SelectQop(qop);

            // assert
            Assert.Equal(expected, selectedQop);
        }
    }
}
