using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace jaytwo.HttpClient.Tests
{
    public class HttpRequestExtensionsTests
    {
        [Theory]
        [InlineData("/foo", "http://google.com", "http://google.com/foo")]
        [InlineData("foo", "http://google.com/", "http://google.com/foo")]
        [InlineData("foo", "http://google.com", "http://google.com/foo")]
        [InlineData("foo?with=query", "http://google.com", "http://google.com/foo?with=query")]
        public void WithBaseUri_after_WithUri(string uri, string baseUri, string expectedUrl)
        {
            // arrange
            var request = new HttpRequest();

            // act
            request.WithUri(uri).WithBaseUri(baseUri);

            // assert
            Assert.Equal(expectedUrl, request.Uri.OriginalString);
        }

        [Theory]
        [InlineData("/foo", "/foo")]
        [InlineData("/foo/bar", "/foo/bar")]
        public void WithPath_without_url(string path, string expectedUrl)
        {
            // arrange
            var request = new HttpRequest();

            // act
            request.WithPath(path);

            // assert
            Assert.Equal(expectedUrl, request.Uri.ToString());
        }

        [Theory]
        [InlineData("foo=bar", "?foo=bar")]
        public void WithQuery_without_url(string query, string expectedUrl)
        {
            // arrange
            var request = new HttpRequest();

            // act
            request.WithQuery(query);

            // assert
            Assert.Equal(expectedUrl, request.Uri.ToString());
        }

        [Theory]
        [InlineData("/hello", "foo=bar", "/hello?foo=bar")]
        public void WithQuery_before_WithPath(string path, string query, string expectedUrl)
        {
            // arrange
            var request = new HttpRequest();

            // act
            request.WithQuery(query).WithPath(path);

            // assert
            Assert.Equal(expectedUrl, request.Uri.ToString());
        }
    }
}
