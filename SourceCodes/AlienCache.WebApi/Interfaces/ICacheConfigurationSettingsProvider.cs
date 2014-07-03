namespace Aliencube.AlienCache.WebApi.Interfaces
{
    /// <summary>
    /// This provides interfaces to the CacheConfigurationSettingsProvider class.
    /// </summary>
    public interface ICacheConfigurationSettingsProvider
    {
        /// <summary>
        /// Gets the duration in seconds, which determines cache to be alive.
        /// </summary>
        int TimeSpan { get; }

        /// <summary>
        /// Gets the value that specifies whether to use absolute URL or not.
        /// </summary>
        bool UseAbsoluteUrl { get; }
    }
}