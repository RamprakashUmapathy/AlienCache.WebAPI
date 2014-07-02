namespace Aliencube.AlienCache.WebApi.Interfaces
{
    /// <summary>
    /// This provides interfaces to the ConfigurationSettings class.
    /// </summary>
    public interface IConfigurationSettings
    {
        /// <summary>
        /// Gets the duration in seconds, which determines cache to be alive.
        /// </summary>
        int TimeSpan { get; }

        /// <summary>
        /// Gets the authentication type.
        /// </summary>
        AuthenticationType AuthenticationType { get; }

        /// <summary>
        /// Gets the username for authentication.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password for authentication.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the authentication key.
        /// </summary>
        string AuthenticationKey { get; }

        /// <summary>
        /// Gets the value that specifies whether to use absolute URL or not.
        /// </summary>
        bool UseAbsoluteUrl { get; }
    }
}