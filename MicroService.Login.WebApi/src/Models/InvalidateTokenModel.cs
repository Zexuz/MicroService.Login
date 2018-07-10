using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    public class InvalidateTokenModel
    {
        /// <summary>
        /// The Refresh Token to invalidate
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
    }
}