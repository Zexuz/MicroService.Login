using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class ReviewRepository : SqlRepositoryBase<SqlReview>, IReviewRepository
    {
        public ReviewRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<List<Review>> GetAsync(int userId, int nrOfItems, int skip)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                var query = "SELECT * FROM [Review] WHERE UserId=@userId ORDER BY Updated OFFSET (@skip) ROWS FETCH NEXT (@nrOfItems) ROWS ONLY";
                var res = await cn.QueryAsync<SqlReview>(query, new {NrOfItems = nrOfItems, UserId = userId, Skip = skip});
                return res.Select(review => review.FromDatabase()).ToList();
            }
        }

        public async Task<List<Review>> FindByIdsAsync(IEnumerable<int> transacrionIds)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                var query = "SELECT * FROM [Review] WHERE Id IN @ids";
                var res = await cn.QueryAsync<SqlReview>(query, new {Ids = transacrionIds});
                return res.Select(review => review.FromDatabase()).ToList();
            }
        }
    }
}