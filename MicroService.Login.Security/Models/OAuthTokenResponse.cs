namespace MicroService.Login.Security.Models
{
    public class OAuthTokenResponse
    {
        /// <summary>
        /// The Json Web Token (JWT)
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The UNIX timestamp in seconds
        /// </summary>
        public long Expires { get; set; }

        /// <summary>
        /// The type of the token
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The Refresh token used for Creating new access tokens
        /// </summary>
        public string RefreshToken { get; set; }
    }
}