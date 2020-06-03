using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace jaytwo.HttpClient.Tests
{
    public class HttpClientTests
    {
        [Fact]
        public async Task Get_Works()
        {
            // arrange
            var client = new HttpClient("http://httpbin.org");

            // act
            var response = await client.GetAsync("/get?hello=world");

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
        }

        [Fact]
        public async Task BasicAuth_Works()
        {
            // arrange
            var client = new HttpClient("http://httpbin.org");
            var user = "hello";
            var pass = "world";

            // act
            var response = await client
                .WithBasicAuthentication(user, pass)
                .GetAsync($"/basic-auth/{user}/{pass}");

            // assert
            var expected = response.EnsureSuccessStatusCode();
        }
    }
}
