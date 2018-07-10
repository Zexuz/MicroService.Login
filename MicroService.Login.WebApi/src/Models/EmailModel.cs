using System.ComponentModel.DataAnnotations;
using MicroService.Common.Core.ValueTypes.ValidationSettings;

namespace MicroService.Login.WebApi.Models
{
    public class EmailModel
    {
        [Required]
        [EmailAddress]
        [StringLength(EmailValidationSettings.MaxLenght)]
        public string Email { get; set; }
    }
}