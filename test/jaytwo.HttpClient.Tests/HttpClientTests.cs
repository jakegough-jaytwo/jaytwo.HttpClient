using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Constants;
using jaytwo.HttpClient.Exceptions;
using jaytwo.MimeHelper;
using Xunit;

namespace jaytwo.HttpClient.Tests
{
    public class HttpClientTests
    {
        private readonly HttpClient _httpClient;

        public HttpClientTests()
        {
            _httpClient = new HttpClient("http://httpbin.org");
        }

        [Fact]
        public async Task RequestHeaders_Work()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync(request =>
            {
                request
                    .WithUri("/headers")
                    .WithHeader("foo", "bar")
                    .WithHeader("fizz", "buzz");
            });

            // assert
            var prototype = new
            {
                headers = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("bar", expected.headers["Foo"]); // don't ask me why, header keys get capitalized
            Assert.Equal("buzz", expected.headers["Fizz"]); // don't ask me why, header keys get capitalized
        }

        [Fact]
        public async Task ResponseHeaders_Work()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync("/response-headers?foo=bar");

            // assert
            Assert.Equal("bar", response.Headers["foo"]);
        }

        [Fact]
        public async Task HttpVersion_10_Works()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync(request =>
            {
                request
                    .WithPath("/get")
                    .WithHttpVersion(HttpVersion.Version10);
            });

            // assert
        }

        [Fact]
        public async Task HttpVersion_11_Works()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync(request =>
            {
                request
                    .WithPath("/get")
                    .WithHttpVersion(HttpVersion.Version11);
            });

            // assert
        }

        [Fact]
        public async Task HttpVersion_20_Works()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync(request =>
            {
                request
                    .WithPath("/get")
                    .WithHttpVersion(HttpVersion.Version20);
            });

            // assert
        }

        [Fact]
        public async Task ResponseHeaders_Work_ContentType()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync("/image/jpeg");

            // assert
            Assert.Equal(MediaType.image_jpeg, response.ContentType);
            Assert.Equal(MediaType.image_jpeg, response.Headers[Headers.ContentType]);
        }

        [Fact]
        public async Task ResponseHeaders_Work_ContentLength()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync("/image/jpeg");

            // assert
            Assert.NotEqual(0, response.ContentLength);
            Assert.NotEqual("0", response.Headers[Headers.ContentLength]);
        }

        [Fact]
        public async Task Get_Works()
        {
            // arrange

            // act
            var response = await _httpClient.GetAsync("/get?hello=world");

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
        }

        [Fact]
        public async Task WithPath_before_WithBaseUri()
        {
            // arrange
            var client = new HttpClient();

            // act
            var response = await client.GetAsync(request =>
            {
                request
                    .WithPath("/get?hello=world")
                    .WithBaseUri("http://httpbin.org");
            });

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Unexpected_MethodNotAllowed_Throws_UnexpectedStatusCodeException()
        {
            // arrange

            // act & assert
            var exception = await Assert.ThrowsAsync<UnexpectedStatusCodeException>(() => _httpClient.DeleteAsync("/get"));

            // assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.StatusCode);
        }

        [Fact]
        public async Task Expected_MethodNotAllowed_DoesNotThrow_UnexpectedStatusCodeException()
        {
            // arrange

            // act & assert
            var response = await _httpClient.DeleteAsync(request =>
            {
                request
                    .WithUri("/get")
                    .WithExpectedStatusCode(HttpStatusCode.MethodNotAllowed);
            });

            // assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task Patch_Works()
        {
            // arrange

            // act
            var response = await _httpClient.PatchAsync(request =>
            {
                request
                    .WithUri("/patch?hello=world")
                    .WithJsonContent(new { fruit = "banana" });
            });

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
                json = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
            Assert.Equal("banana", expected.json["fruit"]);
        }

        [Fact]
        public async Task Post_Works()
        {
            // arrange

            // act
            var response = await _httpClient.PostAsync(request =>
            {
                request
                    .WithUri("/post?hello=world")
                    .WithJsonContent(new { fruit = "banana" });
            });

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
                json = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
            Assert.Equal("banana", expected.json["fruit"]);
        }

        [Fact]
        public async Task Put_Works()
        {
            // arrange

            // act
            var response = await _httpClient.PutAsync(request =>
            {
                request
                    .WithUri("/put?hello=world")
                    .WithJsonContent(new { fruit = "banana" });
            });

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
                json = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
            Assert.Equal("banana", expected.json["fruit"]);
        }

        [Fact]
        public async Task Delete_Works()
        {
            // arrange

            // act
            var response = await _httpClient.DeleteAsync(request =>
            {
                request
                    .WithUri("/delete?hello=world")
                    .WithJsonContent(new { fruit = "banana" });
            });

            // assert
            var prototype = new
            {
                args = default(Dictionary<string, string>),
                json = default(Dictionary<string, string>),
            };

            var expected = response.AsAnonymousType(prototype);
            Assert.Equal("world", expected.args["hello"]);
            Assert.Equal("banana", expected.json["fruit"]);
        }

        [Fact]
        public async Task BasicAuth_Works()
        {
            // arrange
            var user = "hello";
            var pass = "world";

            // act
            var response = await _httpClient
                .WithBasicAuthentication(user, pass)
                .GetAsync($"/basic-auth/{user}/{pass}");

            // assert
            var expected = response.EnsureExpectedStatusCode();
        }

        [Theory]
        [InlineData("auth", "MD5")]
        [InlineData("auth-int", "MD5")]
        // not worth supporting, not even postman or mozilla supports RFC 7616
        //[InlineData("auth", "SHA-256")]
        //[InlineData("auth", "SHA-512")]
        //[InlineData("auth-int", "SHA-256")]
        //[InlineData("auth-int", "SHA-512")]
        public async Task DigestAuth_Works(string qop, string algorithm)
        {
            // arrange
            var user = "hello";
            var pass = "world";

            // act
            var response = await _httpClient.GetAsync(request =>
            {
                request
                    .WithPath($"/digest-auth/{qop}/{user}/{pass}/{algorithm}")
                    .WithDigestAuthentication(user, pass);
            });

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HiddenBasicAuth_Works()
        {
            // arrange
            var user = "hello";
            var pass = "world";

            // act
            var response = await _httpClient
                .WithBasicAuthentication(user, pass)
                .GetAsync($"/hidden-basic-auth/{user}/{pass}");

            // assert
            var expected = response.EnsureExpectedStatusCode();
        }

        [Fact]
        public async Task TokenAuth_Works()
        {
            // arrange
            var token = "hello";

            // act
            var response = await _httpClient
                .WithTokenAuthentication(token)
                .GetAsync($"/bearer");

            // assert
            var expected = response.EnsureExpectedStatusCode();
        }
    }
}
