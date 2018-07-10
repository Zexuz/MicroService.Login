using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface ITransactionRepository : ISqlRepositoryBase<SqlTransaction>
    {
        Task<SqlPaginationResult<Transaction>> GetAsync(int userId, int nrOfItems, int skip);
        Task<SqlPaginationResult<Transaction>> GetSentAsync(int userId, int nrOfItems, int skip);
        Task<SqlPaginationResult<Transaction>> GetReceivedAsync(int userId, int nrOfItems, int skip);
    }
}