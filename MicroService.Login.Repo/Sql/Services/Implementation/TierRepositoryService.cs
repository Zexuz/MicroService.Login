using System.Threading.Tasks;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class TierRepositoryService : ITierRepositoryService
    {
        private readonly ITierRepository _tierRepository;

        public TierRepositoryService(ITierRepository tierRepository)
        {
            _tierRepository = tierRepository;
        }

        public async Task<SqlTier> GetTierAsync(int tierId)
        {
            return await _tierRepository.FindByIdAsync(tierId);
        }
    }
}