using System.ComponentModel.DataAnnotations;

namespace MicroService.Login.WebApi.Models
{
    public class UpdateReviewModel
    {
        [StringLength(500)]
        public string Text { get; set; }

        [Required]
        public bool IsPositive { get; set; }
    }
}