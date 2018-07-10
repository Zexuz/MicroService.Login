using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class ReviewRepositoryService : IReviewRepositoryService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewRepositoryService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }


        public async Task<List<Review>> GetAsync(int userId, int nrOfItems, int skip)
        {
            return await _reviewRepository.GetAsync(userId, nrOfItems, skip);
        }

        public async Task<Review> InsertAsync(Review review, IDbTransaction sqlTransaction)
        {
            var dbRreview = review.ToDatabase();
            if (dbRreview.IsNew)
                throw new ArgumentException("The review must have a id set before inserting.(Use the transactionId)");

            await _reviewRepository.InsertAsync(dbRreview, sqlTransaction);
            return review;
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            return await _reviewRepository.UpdateAsync(review.ToDatabase());
        }

        public async Task<List<Review>> GetAsync(IEnumerable<int> transacrionIds)
        {
            return await _reviewRepository.FindByIdsAsync(transacrionIds);
        }

        public async Task<Review> GetAsync(int userId)
        {
            var res = await _reviewRepository.FindByIdAsync(userId);
            return res.FromDatabase();
        }
    }
}