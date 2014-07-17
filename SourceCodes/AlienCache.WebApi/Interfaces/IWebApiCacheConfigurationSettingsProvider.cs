using System;
using System.Collections.Generic;
using System.Net;

namespace Aliencube.AlienCache.WebApi.Interfaces
{
    /// <summary>
    /// This provides interfaces to the CacheConfigurationSettingsProvider class.
    /// </summary>
    public interface IWebApiCacheConfigurationSettingsProvider : IDisposable
    {
        /// <summary>
        /// Gets the duration in seconds, which determines cache to be alive.
        /// </summary>
        int TimeSpan { get; }

        /// <summary>
        /// Gets the value that specifies whether to use absolute URL or not.
        /// </summary>
        bool UseAbsoluteUrl { get; }

        /// <summary>
        /// Gets the value that specifies whether to use query string as a cache key or not.
        /// </summary>
        bool UseQueryStringAsKey { get; }

        /// <summary>
        /// Gets the key from the query string to be used for cache key.
        /// </summary>
        string QueryStringKey { get; }

        /// <summary>
        /// Gets the list of <c>HttpStatusCode</c>s that are cacheable.
        /// </summary>
        IEnumerable<HttpStatusCode> CacheableStatusCodes { get; }
    }
}