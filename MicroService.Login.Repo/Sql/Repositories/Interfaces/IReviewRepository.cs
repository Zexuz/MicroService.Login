using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using Review = MicroService.Login.Models.RepoService.Review;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface IReviewRepository : ISqlRepositoryBase<Models.SqlReview>
    {
        Task<List<Review>> GetAsync(int userId, int nrOfItems, int skip);
        Task<List<Review>> FindByIdsAsync(IEnumerable<int> transacrionIds);
    }
}