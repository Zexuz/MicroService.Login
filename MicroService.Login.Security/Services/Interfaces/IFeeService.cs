using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IFeeService
    {
        Task<int> CalculateFeeForTransactionAsync(User fromUser, User toUser, int nrOfCoins);
    }
}