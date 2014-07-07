using Aliencube.AlienCache.WebApi.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Aliencube.AlienCache.WebApi
{
    /// <summary>
    /// This represents the cach entity for Web API.
    /// </summary>
    public class WebApiCacheAttribute : ActionFilterAttribute
    {
        private const string RESPONSE_CONTENT_TYPE = "responseContentType";

        private readonly ObjectCache _cache;

        private string _cacheKey;
        private string _responseContentType;

        private Type _cachConfigurationSettingsProviderType;
        private ICacheConfigurationSettingsProvider _settings;

        /// <summary>
        /// Initialises a new instance of the WebApiCacheAttribute class.
        /// </summary>
        public WebApiCacheAttribute()
        {
            this._cache = MemoryCache.Default;
        }

        /// <summary>
        /// Gets or sets the type of the cache configuration settings provider.
        /// </summary>
        public Type CachConfigurationSettingsProviderType
        {
            get
            {
                return this._cachConfigurationSettingsProviderType;
            }
            set
            {
                this._cachConfigurationSettingsProviderType = value;
                this._settings = Activator.CreateInstance(this._cachConfigurationSettingsProviderType) as ICacheConfigurationSettingsProvider;
            }
        }

        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context instance.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException("actionExecutedContext");
            }

            var response = actionExecutedContext.Response;
            if (response == null)
            {
                return;
            }

            var statusCode = response.StatusCode;
            if (!this.IsStatusCodeCacheable(statusCode))
            {
                return;
            }

            var content = response.Content;
            if (content == null)
            {
                return;
            }

            var now = DateTime.Now;
            var callback = this.GetCallbackFunction(actionExecutedContext.Request);

            this.AddResponseToCache(content, callback, now);
            this.AddContentTypeToCache(content, now);
            this.AddCacheHeaderControl(actionExecutedContext);

            base.OnActionExecuted(actionExecutedContext);
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (!this.IsCacheable(actionContext))
            {
                return;
            }

            this.SetCacheKeys(actionContext);

            if (!this._cache.Contains(this._cacheKey))
            {
                return;
            }

            var response = this.GetResponseFromCache(actionContext);
            if (response == null)
            {
                return;
            }

            actionContext.Response = response;

            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// Checks whether the status code is cacheable.
        /// </summary>
        /// <param name="statusCode"><c>HttpStatusCode</c> instance.</param>
        /// <returns>Returns <c>True</c>, if the status code is cacheable; otherwise returns <c>False</c>.</returns>
        private bool IsStatusCodeCacheable(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.InternalServerError || statusCode == HttpStatusCode.ServiceUnavailable)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the callback function name.
        /// </summary>
        /// <param name="request"><c>HttpRequestMessage</c> instance.</param>
        /// <returns>Returns the callback function name.</returns>
        private string GetCallbackFunction(HttpRequestMessage request)
        {
            var qs = request.RequestUri.ParseQueryString();
            var callback = qs.Get("callback");
            return callback;
        }

        /// <summary>
        /// Adds response data to the cache.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <param name="now">Current <c>DateTime</c> instance.</param>
        private void AddResponseToCache(HttpContent content, string callback, DateTime now)
        {
            if (this._cache.Contains(this._cacheKey))
            {
                return;
            }

            var body = content.ReadAsStringAsync().Result;
            if (content.Headers.ContentType.MediaType == "text/javascript")
            {
                body = body.Replace(callback, "");
                var index = body.IndexOf("(", StringComparison.Ordinal);
                var lastIndex = body.LastIndexOf(")", StringComparison.Ordinal);
                body = body.Substring(index + 1, lastIndex - index - 1);
            }
            this._cache.Add(this._cacheKey, body, now.AddSeconds(this._settings.TimeSpan));
        }

        /// <summary>
        /// Adds content type to the cache.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="now">Current <c>DateTime</c> instance.</param>
        private void AddContentTypeToCache(HttpContent content, DateTime now)
        {
            if (this._cache.Contains(this._responseContentType))
            {
                return;
            }

            var contentType = new MediaTypeHeaderValue("text/plain");

            var headers = content.Headers;
            if (headers != null && headers.ContentType != null)
                contentType = headers.ContentType;

            this._cache.Add(this._responseContentType, contentType, now.AddSeconds(this._settings.TimeSpan));
        }

        /// <summary>
        /// Adds cache header control.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context instance.</param>
        private void AddCacheHeaderControl(HttpActionExecutedContext actionExecutedContext)
        {
            if (!this.IsCacheable(actionExecutedContext.ActionContext))
            {
                return;
            }
            actionExecutedContext.ActionContext.Response.Headers.CacheControl = this.GetClientCache();
        }

        /// <summary>
        /// Checks whether the request is cacheable or not.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns <c>True</c>, if the request is cacheable; otherwise returns <c>False</c>.</returns>
        private bool IsCacheable(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (actionContext.Request.Method != HttpMethod.Get)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the cache keys based on URL and MIME-Type.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        private void SetCacheKeys(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var path = this._settings.UseAbsoluteUrl
                ? actionContext.Request.RequestUri.AbsoluteUri
                : actionContext.Request.RequestUri.AbsolutePath;

            var accept = actionContext.Request.Headers.Accept.FirstOrDefault();
            var mimeType = accept == null ? "text/plain" : accept.ToString();

            var cacheKey = String.Join("|", path, mimeType);
            var responseContentType = String.Join("|", path, mimeType, RESPONSE_CONTENT_TYPE);

            this._cacheKey = cacheKey;
            this._responseContentType = responseContentType;
        }

        /// <summary>
        /// Gets the response from the cache.
        /// </summary>
        /// <param name="actionContext">The action context instance.</param>
        /// <returns>Returns the response from the cache.</returns>
        private HttpResponseMessage GetResponseFromCache(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var value = this._cache.Get(this._cacheKey) as string;
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var response = actionContext.Request.CreateResponse(HttpStatusCode.OK);
            var content = new StringContent(value);
            var contentType = this._cache.Get(this._responseContentType) as MediaTypeHeaderValue ??
                              new MediaTypeHeaderValue(this._cacheKey.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[1]) { CharSet = "utf-8" };

            content.Headers.ContentType = contentType;
            content.Headers.Add("X-Aliencube-Cached", "Cached");
            response.Content = content;
            response.Headers.CacheControl = this.GetClientCache();

            return response;
        }

        /// <summary>
        /// Gets the client cache.
        /// </summary>
        /// <returns>Returns the client cache.</returns>
        private CacheControlHeaderValue GetClientCache()
        {
            return new CacheControlHeaderValue { MaxAge = System.TimeSpan.FromSeconds(this._settings.TimeSpan), MustRevalidate = true };
        }
    }
}