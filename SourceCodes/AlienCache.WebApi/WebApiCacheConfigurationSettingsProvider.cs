using Aliencube.AlienCache.WebApi.Interfaces;
using Aliencube.AlienCache.WebApi.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Aliencube.AlienCache.WebApi
{
    /// <summary>
    /// This represents the entity providing configuration settings.
    /// </summary>
    public class WebApiCacheConfigurationSettingsProvider : IWebApiCacheConfigurationSettingsProvider
    {
        private readonly Settings _settings;

        /// <summary>
        /// Initialises a new instance of the CacheConfigurationSettingsProvider class.
        /// </summary>
        public WebApiCacheConfigurationSettingsProvider()
        {
            this._settings = Settings.Default;
        }

        /// <summary>
        /// Gets the duration in seconds, which determines cache to be alive.
        /// </summary>
        public int TimeSpan
        {
            get { return this._settings.TimeSpan; }
        }

        /// <summary>
        /// Gets the value that specifies whether to use absolute URL or not.
        /// </summary>
        public bool UseAbsoluteUrl
        {
            get { return this._settings.UseAbsoluteUrl; }
        }

        /// <summary>
        /// Gets the value that specifies whether to use query string as a cache key or not.
        /// </summary>
        public bool UseQueryStringAsKey
        {
            get { return this._settings.UseQueryStringAsKey; }
        }

        /// <summary>
        /// Gets the key from the query string to be used for cache key.
        /// </summary>
        public string QueryStringKey
        {
            get { return this._settings.QueryStringKey; }
        }

        /// <summary>
        /// Gets the list of <c>HttpStatusCode</c>s that are cacheable.
        /// </summary>
        public IEnumerable<HttpStatusCode> CacheableStatusCodes
        {
            get
            {
                HttpStatusCode result;
                var codes = this._settings
                                .CacheableStatusCodes
                                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => Enum.TryParse(p, true, out result) ? result : HttpStatusCode.InternalServerError)
                                .Where(p => p != HttpStatusCode.InternalServerError);
                return codes;
            }
        }
    }
}