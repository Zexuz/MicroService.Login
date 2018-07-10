using System.Data;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class TransactionRepositoryService : ITransactionRepositoryService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionRepositoryService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<SqlPaginationResult<Transaction>> GetAsync(int userId, int nrOfItems, int skip)
        {
            return await _transactionRepository.GetAsync(userId, nrOfItems, skip);
        }

        public async Task<SqlPaginationResult<Transaction>> GetSentAsync(int userId, int nrOfItems, int skip)
        {
            return await _transactionRepository.GetSentAsync(userId, nrOfItems, skip);
        }

        public async Task<SqlPaginationResult<Transaction>> GetReceivedAsync(int userId, int nrOfItems, int skip)
        {
            return await _transactionRepository.GetReceivedAsync(userId, nrOfItems, skip);
        }

        public async Task<int> InsertAsync(Transaction transaction, IDbTransaction sqlTransaction)
        {
            return await _transactionRepository.InsertAsync(transaction.ToDatabase(), sqlTransaction);
        }
    }
}