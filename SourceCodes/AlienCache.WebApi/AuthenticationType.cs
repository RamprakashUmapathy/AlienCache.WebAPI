namespace Aliencube.AlienCache.WebApi
{
    /// <summary>
    /// This specifies the authentication type.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// Identifies the authentiction type is not determined or no authentication is required.
        /// </summary>
        Anonymous = 0,

        /// <summary>
        /// Identifies basic username and password are used for authentication.
        /// </summary>
        Basic = 1,

        /// <summary>
        /// Identifies auth key is used for authentication.
        /// </summary>
        AuthKey = 2,
    }
}