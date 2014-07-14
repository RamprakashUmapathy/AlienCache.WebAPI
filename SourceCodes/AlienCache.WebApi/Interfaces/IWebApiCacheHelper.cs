using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace Aliencube.AlienCache.WebApi.Interfaces
{
    /// <summary>
    /// This provides interfaces to the WebApiCacheAttribute class.
    /// </summary>
    public interface IWebApiCacheHelper : IDisposable
    {
        /// <summary>
        /// Gets the web API cache configuration settings.
        /// </summary>
        IWebApiCacheConfigurationSettingsProvider Settings { get; }

        /// <summary>
        /// Checks whether the status code is cacheable.
        /// </summary>
        /// <param name="statusCode"><c>HttpStatusCode</c> instance.</param>
        /// <returns>Returns <c>True</c>, if the status code is cacheable; otherwise returns <c>False</c>.</returns>
        bool IsStatusCodeCacheable(HttpStatusCode statusCode);

        /// <summary>
        /// Gets the callback function name.
        /// </summary>
        /// <param name="request"><c>HttpRequestMessage</c> instance.</param>
        /// <returns>Returns the callback function name.</returns>
        string GetCallbackFunction(HttpRequestMessage request);

        /// <summary>
        /// Checks whether the request is cacheable or not.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns <c>True</c>, if the request is cacheable; otherwise returns <c>False</c>.</returns>
        bool IsCacheable(HttpActionContext actionContext);

        /// <summary>
        /// Gets the content from the response content.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <returns>Returns the content from the response content.</returns>
        string GetResponseContent(HttpContent content, string callback);

        /// <summary>
        /// Gets the content type from the content header.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <returns>Returns the content type from the content header.</returns>
        MediaTypeHeaderValue GetContentHeaderContentType(HttpContent content);

        /// <summary>
        /// Gets the client cache.
        /// </summary>
        /// <returns>Returns the client cache.</returns>
        CacheControlHeaderValue GetClientCache();

        /// <summary>
        /// Gets the request URL path.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns the request URL path.</returns>
        string GetActionPath(HttpActionContext actionContext);

        /// <summary>
        /// Checks whether the reqeust contains JSONP callback function or not.
        /// </summary>
        /// <param name="request"><c>HttpRequestMessage</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <returns>Returns <c>True</c>, if the request contains JSONP callback function; otherwise returns <c>False</c>.</returns>
        bool IsJsonpRequest(HttpRequestMessage request, out string callback);
    }
}