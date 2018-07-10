using System.ComponentModel.DataAnnotations;
using MicroService.Common.Core.ValueTypes.ValidationSettings;

namespace MicroService.Login.WebApi.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(18)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(EmailValidationSettings.MaxLenght)]
        public string Email { get; set; }

        [Required]
//        [StringLength(100,MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        public string PasswordRepeat { get; set; }
    }
}