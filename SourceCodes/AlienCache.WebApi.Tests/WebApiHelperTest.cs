using Aliencube.AlienCache.WebApi.Interfaces;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Aliencube.AlienCache.WebApi.Tests
{
    [TestFixture]
    public class WebApiCacheHelperTest
    {
        #region SetUp / TearDown

        private HttpRequestMessage _request;
        private IWebApiCacheHelper _helper;

        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public void Dispose()
        {
            if (this._request != null)
                this._request.Dispose();

            if (this._helper != null)
                this._helper.Dispose();
        }

        #endregion SetUp / TearDown

        #region Tests

        [Test]
        [TestCase(200, true)]
        [TestCase(304, true)]
        [TestCase(500, false)]
        public void IsStatusCodeCacheable_GivenConfig_ReturnResult(HttpStatusCode statusCode, bool expected)
        {
            var cacheableStatusCodes = new List<HttpStatusCode>
                                       {
                                           HttpStatusCode.OK,
                                           HttpStatusCode.NotModified
                                       };

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            settings.CacheableStatusCodes.Returns(cacheableStatusCodes);

            this._helper = new WebApiCacheHelper(settings);

            this._helper.IsStatusCodeCacheable(statusCode).Should().Be(expected);
        }

        [Test]
        [TestCase("", null)]
        [TestCase("callback", "")]
        [TestCase("callback=", "")]
        [TestCase("callback=jQuery_1234567890", "jQuery_1234567890")]
        public void GetCallbackFunction_GivenRequest_ReturnCallbackFunction(string qs, string expected)
        {
            var method = new HttpMethod("GET");
            var url = String.Format("http://localhost{0}", (String.IsNullOrWhiteSpace(qs) ? null : ("?" + qs)));
            var uri = new Uri(url);
            this._request = new HttpRequestMessage(method, uri);

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            this._helper = new WebApiCacheHelper(settings);

            this._helper.GetCallbackFunction(this._request).Should().Be(expected);
        }

        [Test]
        [TestCase("GET", true)]
        [TestCase("POST", false)]
        [TestCase("OPTIONS", true)]
        public void IsCacheable_GivenRequest_ReturnResult(string method, bool expected)
        {
            var url = "http://localhost";
            var uri = new Uri(url);
            this._request = new HttpRequestMessage(new HttpMethod(method), uri);

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            this._helper = new WebApiCacheHelper(settings);

            var actionContext = ContextUtil.GetActionContext(this._request);

            this._helper.IsCacheable(actionContext).Should().Be(expected);
        }

        [Test]
        [TestCase("text/javascript", "jQuery_1234567890", "jQuery_1234567890(RESPONSE)", "RESPONSE")]
        [TestCase("application/json", "", "RESPONSE", "RESPONSE")]
        [TestCase("text/plain", "", "RESPONSE", "RESPONSE")]
        [TestCase("text/html", "", "RESPONSE", "RESPONSE")]
        [TestCase("application/json", "jQuery_1234567890", "RESPONSE", "RESPONSE")]
        public void GetResponseContent_GivenContent_ReturnResponseContent(string mediaType, string callback, string body, string expected)
        {
            var content = new StringContent(body);
            content.Headers.ContentType.MediaType = mediaType;

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            this._helper = new WebApiCacheHelper(settings);

            this._helper.GetResponseContent(content, callback).Should().Be(expected);
        }

        [Test]
        [TestCase("", "text/plain")]
        [TestCase("text/html", "text/html")]
        [TestCase("application/json", "application/json")]
        public void GetContentHeaderContentType_GivenContent_ReturnContentMediaType(string mediaType, string expected)
        {
            var content = new StringContent("RESPONSE");
            if (!String.IsNullOrWhiteSpace(mediaType))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            }

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            this._helper = new WebApiCacheHelper(settings);

            this._helper.GetContentHeaderContentType(content).MediaType.Should().Be(expected);
        }

        [Test]
        [TestCase("/v3?url=repo/abc/def", false, "", "/v3")]
        [TestCase("/v3?url=repo/abc/def", true, "url", "/v3/repo/abc/def")]
        [TestCase("/v3?url=repo/abc/def", true, "abc", "/v3")]
        public void GetActionPath_GivenActionContext_ReturnActionPath(string path, bool useQueryStringAsKey, string queryStringKey, string expected)
        {
            this._request = new HttpRequestMessage(new HttpMethod("GET"), (path.StartsWith("/")
                                                                                ? new Uri("http://localhost" + path)
                                                                                : new Uri(path)));

            var actionContext = ContextUtil.GetActionContext(this._request);

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();
            settings.UseAbsoluteUrl.Returns(false);
            settings.UseQueryStringAsKey.Returns(useQueryStringAsKey);
            settings.QueryStringKey.Returns(queryStringKey);

            this._helper = new WebApiCacheHelper(settings);
            this._helper.GetActionPath(actionContext).Should().Be(expected);
        }

        [Test]
        [TestCase("", null, false)]
        [TestCase("jQuery_1234567890", "jQuery_1234567890", true)]
        public void IsJsonRequest_GivenRequest_ReturnValue(string callbackName, string found, bool expected)
        {
            var path = "http://localhost" + (String.IsNullOrWhiteSpace(callbackName) ? null : "?callback=" + callbackName);
            this._request = new HttpRequestMessage(new HttpMethod("GET"), path);

            var settings = Substitute.For<IWebApiCacheConfigurationSettingsProvider>();

            this._helper = new WebApiCacheHelper(settings);

            string callback;
            this._helper.IsJsonpRequest(this._request, out callback).Should().Be(expected);
            callback.Should().Be(found);
        }

        #endregion Tests
    }
}