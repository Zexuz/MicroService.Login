using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    public class AddTwoFactorModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Token { get; set; }
    }
}