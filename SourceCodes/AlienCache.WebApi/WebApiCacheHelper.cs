using Aliencube.AlienCache.WebApi.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace Aliencube.AlienCache.WebApi
{
    /// <summary>
    /// This represents an entity to help <c>WebApiCacheAttribute</c> class.
    /// </summary>
    public class WebApiCacheHelper : IWebApiCacheHelper
    {
        /// <summary>
        /// Initialises a new instance of the WebApiCacheHelper class.
        /// </summary>
        /// <param name="settings">Web API cache configuration settings.</param>
        public WebApiCacheHelper(IWebApiCacheConfigurationSettingsProvider settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.Settings = settings;
        }

        /// <summary>
        /// Gets the web API cache configuration settings.
        /// </summary>
        public IWebApiCacheConfigurationSettingsProvider Settings { get; private set; }

        /// <summary>
        /// Checks whether the status code is cacheable.
        /// </summary>
        /// <param name="statusCode"><c>HttpStatusCode</c> instance.</param>
        /// <returns>Returns <c>True</c>, if the status code is cacheable; otherwise returns <c>False</c>.</returns>
        public bool IsStatusCodeCacheable(HttpStatusCode statusCode)
        {
            var cacheable = this.Settings
                                .CacheableStatusCodes
                                .Contains(statusCode);
            return cacheable;
        }

        /// <summary>
        /// Gets the callback function name.
        /// </summary>
        /// <param name="request"><c>HttpRequestMessage</c> instance.</param>
        /// <returns>Returns the callback function name.</returns>
        public string GetCallbackFunction(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var qs = request.RequestUri.ParseQueryString();
            var callback = qs.Get("callback");
            return callback;
        }

        /// <summary>
        /// Checks whether the request is cacheable or not.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns <c>True</c>, if the request is cacheable; otherwise returns <c>False</c>.</returns>
        public bool IsCacheable(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var method = actionContext.Request.Method;
            var cacheable = method == HttpMethod.Get || method == HttpMethod.Options;
            return cacheable;
        }

        /// <summary>
        /// Gets the content from the response content.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <returns>Returns the content from the response content.</returns>
        public string GetResponseContent(HttpContent content, string callback)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            var body = content.ReadAsStringAsync().Result;

            if (content.Headers.ContentType.MediaType != "text/javascript")
            {
                return body;
            }

            //  Wraps response with callback function for JSONP request.

            if (String.IsNullOrWhiteSpace(callback))
            {
                throw new ArgumentNullException("callback");
            }

            body = body.Replace(callback, "");
            var index = body.IndexOf("(", StringComparison.Ordinal);
            var lastIndex = body.LastIndexOf(")", StringComparison.Ordinal);
            body = body.Substring(index + 1, lastIndex - index - 1);

            return body;
        }

        /// <summary>
        /// Gets the content type from the content header.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <returns>Returns the content type from the content header.</returns>
        public MediaTypeHeaderValue GetContentHeaderContentType(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            var contentType = new MediaTypeHeaderValue("text/plain");

            var headers = content.Headers;
            if (headers != null && headers.ContentType != null)
                contentType = headers.ContentType;

            return contentType;
        }

        /// <summary>
        /// Gets the client cache.
        /// </summary>
        /// <returns>Returns the client cache.</returns>
        public CacheControlHeaderValue GetClientCache()
        {
            return new CacheControlHeaderValue { MaxAge = System.TimeSpan.FromSeconds(this.Settings.TimeSpan), MustRevalidate = true };
        }

        /// <summary>
        /// Gets the request URL path.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns the request URL path.</returns>
        public string GetActionPath(HttpActionContext actionContext)
        {
            var path = this.Settings.UseAbsoluteUrl
                ? actionContext.Request.RequestUri.AbsoluteUri
                : actionContext.Request.RequestUri.AbsolutePath;

            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            path += this.GetCacheKeyFromQueryString(actionContext);

            return path;
        }

        /// <summary>
        /// Gets the cache key from the query string.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns the cache key from the query string.</returns>
        public string GetCacheKeyFromQueryString(HttpActionContext actionContext)
        {
            if (!this.Settings.UseQueryStringAsKey)
            {
                return null;
            }

            var key = this.Settings.QueryStringKey;
            if (String.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var value = actionContext.Request.RequestUri.ParseQueryString().Get(key);
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!value.StartsWith("/"))
            {
                value = "/" + value;
            }

            return value;
        }

        /// <summary>
        /// Checks whether the reqeust contains JSONP callback function or not.
        /// </summary>
        /// <param name="request"><c>HttpRequestMessage</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <returns>Returns <c>True</c>, if the request contains JSONP callback function; otherwise returns <c>False</c>.</returns>
        public bool IsJsonpRequest(HttpRequestMessage request, out string callback)
        {
            var qs = request.RequestUri.ParseQueryString();
            callback = qs.Get("callback");
            return !String.IsNullOrWhiteSpace(callback);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}