using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    public class ResetPasswordModel
    {
        [Required]
//        [StringLength(100,MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        public string PasswordRepeat { get; set; }
    }
}