using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface IReviewRepositoryService
    {
        Task<List<Review>> GetAsync(int userId, int nrOfItems, int skip);

        Task<Review> InsertAsync(Review review, IDbTransaction sqlTransaction);

        Task<bool>         UpdateReviewAsync(Review review);
        Task<List<Review>> GetAsync(IEnumerable<int> transacrionIds);
        Task<Review>       GetAsync(int userId);
    }
}