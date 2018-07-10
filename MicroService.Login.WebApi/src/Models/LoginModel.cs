using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    /// <summary>
    /// Model for sending a login request
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Users email
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// User password as plain text
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// The two factor code from the clients device.
        /// Only needed if the client has enabled 2fa on the account.
        /// </summary>
        public string TwoFactorCode { get; set; }
    }
}