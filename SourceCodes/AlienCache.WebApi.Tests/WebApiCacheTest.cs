using NUnit.Framework;
using System;
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
        }

        #endregion SetUp / TearDown

        #region Tests

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