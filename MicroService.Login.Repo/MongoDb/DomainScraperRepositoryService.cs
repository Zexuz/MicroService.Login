using System.Linq;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository;
using MicroService.Login.Repo.MongoDb.Models;

namespace MicroService.Login.Repo.MongoDb
{
    public class DomainScraperRepositoryService : IDomainScraperRepositoryService
    {
        private IRepository<DomainScraper, string> _repository;

        public DomainScraperRepositoryService(IRepository<DomainScraper, string> repository)
        {
            _repository = repository;
        }

        public async Task DeleteAsync(string id)
        {
            await _repository.Delete(id);
        }

        public async Task InsertAsync(DomainScraper domainScraper)
        {
            await _repository.SaveAsync(domainScraper);
        }

        public async Task<DomainScraper> FindByUserId(int userId)
        {
            var all = await _repository.GetAll();
            return all.SingleOrDefault(mongo => mongo.UserId == userId);
        }
    }
}