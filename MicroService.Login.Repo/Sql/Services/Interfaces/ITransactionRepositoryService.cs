using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface ITransactionRepositoryService
    {
        Task<SqlPaginationResult<Transaction>> GetAsync(int userId, int nrOfItems, int skip);
        Task<SqlPaginationResult<Transaction>> GetSentAsync(int userId, int nrOfItems, int skip);
        Task<SqlPaginationResult<Transaction>> GetReceivedAsync(int userId, int nrOfItems, int skip);

        Task<int> InsertAsync(Transaction transaction, IDbTransaction sqlTransaction);
    }

    public class SqlPaginationResult<TEntity>
    {
        public long          TotalCount { get; set; }
        public List<TEntity> Data       { get; set; }
    }
}