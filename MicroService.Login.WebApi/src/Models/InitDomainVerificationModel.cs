using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    public class InitDomainVerificationModel
    {
        [Required]
        [StringLength(100)]
        public string Website{ get; set; }
    }
}