using Aliencube.AlienCache.WebApi.Interfaces;
using Aliencube.AlienCache.WebApi.Properties;

namespace Aliencube.AlienCache.WebApi
{
    /// <summary>
    /// This represents the entity providing configuration settings.
    /// </summary>
    public class CacheConfigurationSettingsProvider : ICacheConfigurationSettingsProvider
    {
        private readonly Settings _settings;

        /// <summary>
        /// Initialises a new instance of the CacheConfigurationSettingsProvider class.
        /// </summary>
        public CacheConfigurationSettingsProvider()
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
    }
}