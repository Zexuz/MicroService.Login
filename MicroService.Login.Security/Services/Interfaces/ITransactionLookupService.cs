using System.Threading.Tasks;
using MicroService.Login.Models.ViewModel;
using MicroService.Login.Repo;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface ITransactionLookupService
    {
        Task<PaginatedItemsModel<TransactionViewModel>> GetAsync(int userId, int page);
        Task<PaginatedItemsModel<TransactionViewModel>> GetSentAsync(int userId, int page);
        Task<PaginatedItemsModel<TransactionViewModel>> GetReceivedAsync(int userId, int page);
    }
}