using System;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class FeeService : IFeeService
    {
        private readonly ITierRepositoryService _tierRepositoryService;

        public FeeService(ITierRepositoryService tierRepositoryService)
        {
            _tierRepositoryService = tierRepositoryService;
        }

        public async Task<int> CalculateFeeForTransactionAsync(User fromUser, User toUser, int nrOfCoins)
        {
            var fromUserTier = await _tierRepositoryService.GetTierAsync(fromUser.TierId);
            var toUserTier = await _tierRepositoryService.GetTierAsync(toUser.TierId);


            if (fromUser.DomainId != null)
            {
                return Ceiling(nrOfCoins, fromUserTier.TransferFee);
            }

            if (toUser.DomainId != null)
            {
                return Ceiling(nrOfCoins, toUserTier.TransferFee);
            }

            return Ceiling(nrOfCoins, fromUserTier.TransferFee);
        }

        private static int Ceiling(int nrOfCoins, decimal fee)
        {
            return (int) Math.Ceiling((fee / 100) * nrOfCoins);
        }
    }
}