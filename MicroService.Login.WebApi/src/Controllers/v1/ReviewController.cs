using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.WebApi.Models;

namespace MicroService.Login.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class ReviewController : Controller
    {
        private readonly IReviewRepositoryService _reviewRepositoryService;

        public ReviewController(IReviewRepositoryService reviewRepositoryService)
        {
            _reviewRepositoryService = reviewRepositoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> LookupReview(int id)
        {
            var review = await _reviewRepositoryService.GetAsync(id);
            if (review == null) return NotFound();

            return Ok(review);
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewModel model, int id)
        {
            var review = await _reviewRepositoryService.GetAsync(id);
            if (review == null) return NotFound();

            if (review.UserId != int.Parse(User.Identity.Name))
                return Forbid();

            review.IsPositive = model.IsPositive;
            review.Text = model.Text;
            review.Updated = DateTimeOffset.Now;
            if (await _reviewRepositoryService.UpdateReviewAsync(review))
                return Ok(review);

            var ex = new Exception("");
            ex.Data.Add("review-id", review.Id);
            throw ex;
        }
    }
}