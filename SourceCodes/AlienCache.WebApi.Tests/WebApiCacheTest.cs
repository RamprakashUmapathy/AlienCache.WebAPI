using System.Security.Policy;
using Aliencube.AlienCache.WebApi.Interfaces;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Aliencube.AlienCache.WebApi.Tests
{
    [TestFixture]
    public class WebApiCacheTest
    {
        #region SetUp / TearDown

        private HttpRequestMessage _request;
        private HttpResponseMessage _response;
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

            if (this._response != null)
                this._response.Dispose();

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
        [TestCase("GET", "/repos/user/repo/git/refs/heads/master", null)]
        public void OnActionExecuting_GivenRequest_ReturnResponse(string method, string path, object expected)
        {
            this._request = new HttpRequestMessage(new HttpMethod(method), (path.StartsWith("/")
                                                                                ? new Uri("http://localhost" + path)
                                                                                : new Uri(path)));

            var actionContext = ContextUtil.GetActionContext(this._request);

            var cache = new WebApiCacheAttribute();
            cache.OnActionExecuting(actionContext);

            Assert.AreEqual(expected, actionContext.Response);
        }

        [Test]
        [TestCase("GET", "/repos/user/repo/git/refs/heads/master", "response contents")]
        public void OnActionExtecuted_GivenRequestAndResponse_ReturnResponse(string method, string path, string expected)
        {
            this._request = new HttpRequestMessage(new HttpMethod(method), (path.StartsWith("/")
                                                                                ? new Uri("http://localhost" + path)
                                                                                : new Uri(path)));
            this._response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expected) };

            var actionContext = ContextUtil.GetActionContext(this._request);
            var actionExecutedContext = ContextUtil.GetActionExecutedContext(this._request, this._response);

            var cache = new WebApiCacheAttribute();
            cache.OnActionExecuting(actionContext);
            cache.OnActionExecuted(actionExecutedContext);

            Assert.AreEqual(expected, actionExecutedContext.Response.Content.ReadAsStringAsync().Result);
        }

        #endregion Tests
    }
}