using System.Threading.Tasks;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<bool> SendCoinsAsync(int fromUserId, int toUserId, int ammount);
    }
}