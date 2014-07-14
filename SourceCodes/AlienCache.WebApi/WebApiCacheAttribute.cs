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

        private Type _webApiCacheConfigurationSettingsProviderType;
        private IWebApiCacheConfigurationSettingsProvider _settings;
        private IWebApiCacheHelper _helper;

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
        public Type WebApiCacheConfigurationSettingsProviderType
        {
            get
            {
                return this._webApiCacheConfigurationSettingsProviderType;
            }
            set
            {
                this._webApiCacheConfigurationSettingsProviderType = value;
                this._settings = Activator.CreateInstance(this._webApiCacheConfigurationSettingsProviderType) as IWebApiCacheConfigurationSettingsProvider;
                this._helper = new WebApiCacheHelper(this._settings);
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
            if (!this._helper.IsStatusCodeCacheable(statusCode))
            {
                return;
            }

            var content = response.Content;
            if (content == null)
            {
                return;
            }

            var now = DateTime.Now;
            var callback = this._helper.GetCallbackFunction(actionExecutedContext.Request);

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

            if (!this._helper.IsCacheable(actionContext))
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
        /// Adds response data to the cache.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="callback">Callback function name.</param>
        /// <param name="now">Current <c>DateTime</c> instance.</param>
        private void AddResponseToCache(HttpContent content, string callback, DateTime now)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (now == null || now == DateTime.MinValue)
            {
                throw new ArgumentNullException("now");
            }

            if (this._cache.Contains(this._cacheKey))
            {
                return;
            }

            var body = this._helper.GetResponseContent(content, callback);
            this._cache.Add(this._cacheKey, body, now.AddSeconds(this._settings.TimeSpan));
        }

        /// <summary>
        /// Adds content type to the cache.
        /// </summary>
        /// <param name="content"><c>HttpContent</c> instance.</param>
        /// <param name="now">Current <c>DateTime</c> instance.</param>
        private void AddContentTypeToCache(HttpContent content, DateTime now)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (now == null || now == DateTime.MinValue)
            {
                throw new ArgumentNullException("now");
            }

            if (this._cache.Contains(this._responseContentType))
            {
                return;
            }

            var contentType = this._helper.GetContentHeaderContentType(content);

            this._cache.Add(this._responseContentType, contentType, now.AddSeconds(this._settings.TimeSpan));
        }

        /// <summary>
        /// Adds cache header control.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context instance.</param>
        private void AddCacheHeaderControl(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException("actionExecutedContext");
            }

            if (!this._helper.IsCacheable(actionExecutedContext.ActionContext))
            {
                return;
            }

            actionExecutedContext.ActionContext.Response.Headers.CacheControl = this._helper.GetClientCache();
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

            var path = this._helper.GetActionPath(actionContext);

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

            var content = new StringContent(value);
            var contentType = this._cache.Get(this._responseContentType) as MediaTypeHeaderValue ??
                              new MediaTypeHeaderValue(this._cacheKey.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[1]) { CharSet = "utf-8" };

            string callback;
            if (this._helper.IsJsonpRequest(actionContext.Request, out callback))
            {
                content = new StringContent(String.Format("{0}({1})", callback, value));
                contentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "utf-8" };
            }

            content.Headers.ContentType = contentType;
            content.Headers.Add("X-Aliencube-Cached", "Cached");

            var response = actionContext.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = content;
            response.Headers.CacheControl = this._helper.GetClientCache();

            return response;
        }
    }
}