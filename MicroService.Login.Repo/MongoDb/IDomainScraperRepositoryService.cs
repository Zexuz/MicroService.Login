using System.Threading.Tasks;
using MicroService.Login.Repo.MongoDb.Models;

namespace MicroService.Login.Repo.MongoDb
{
    public interface IDomainScraperRepositoryService
    {
        Task                DeleteAsync(string id);
        Task                InsertAsync(DomainScraper domainScraper);
        Task<DomainScraper> FindByUserId(int userId);
    }
}