using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
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

        private int _timespan;
        private AuthenticationType _authenticationType;
        private string _username;
        private string _password;
        private string _authKey;
        private bool _useAbsoluteUrl;

        private string _cacheKey;
        private string _responseContentType;

        /// <summary>
        /// Initialises a new instance of the WebApiCache class.
        /// </summary>
        public WebApiCacheAttribute()
        {
            this._timespan = 60;
            this._authenticationType = AuthenticationType.Anonymous;
            this._useAbsoluteUrl = false;
            this._cache = MemoryCache.Default;
        }

        /// <summary>
        /// Gets or sets the duration in seconds, which determines cache to be alive.
        /// </summary>
        public int TimeSpan
        {
            get { return this._timespan; }
            set
            {
                if (value <= 0)
                    value = 60;

                this._timespan = value;
            }
        }

        /// <summary>
        /// Gets or sets the authentication type.
        /// </summary>
        public AuthenticationType AuthenticationType
        {
            get { return this._authenticationType; }
            set { this._authenticationType = value; }
        }

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string Username
        {
            get { return this._username; }
            set { this._username = value; }
        }

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

        /// <summary>
        /// Gets or sets the auth key for authentication.
        /// </summary>
        public string AuthKey
        {
            get { return this._authKey; }
            set { this._authKey = value; }
        }

        /// <summary>
        /// Gets or sets the value that specifies whether to use absolute URL or not.
        /// </summary>
        public bool UseAbsoluteUrl
        {
            get { return this._useAbsoluteUrl; }
            set { this._useAbsoluteUrl = value; }
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

            var content = response.Content;
            if (content == null)
            {
                return;
            }

            var now = DateTime.Now;

            if (!this._cache.Contains(this._cacheKey))
            {
                var body = content.ReadAsStringAsync().Result;
                this._cache.Add(this._cacheKey, body, now.AddSeconds(this._timespan));
            }

            if (!this._cache.Contains(this._responseContentType))
            {
                var contentType = new MediaTypeHeaderValue("text/plain");

                var headers = content.Headers;
                if (headers != null && headers.ContentType != null)
                    contentType = headers.ContentType;

                this._cache.Add(this._responseContentType, contentType, now.AddSeconds(this._timespan));
            }

            if (this.IsCacheable(actionExecutedContext.ActionContext))
            {
                actionExecutedContext.ActionContext.Response.Headers.CacheControl = this.GetClientCache();
            }

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

            if (this.AuthenticationType == AuthenticationType.Basic && !Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return false;
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

            var path = this.UseAbsoluteUrl
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
            return new CacheControlHeaderValue { MaxAge = System.TimeSpan.FromSeconds(this.TimeSpan), MustRevalidate = true };
        }
    }
}