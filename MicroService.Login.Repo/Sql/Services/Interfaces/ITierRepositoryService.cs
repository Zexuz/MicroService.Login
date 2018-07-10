using System.Threading.Tasks;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface ITierRepositoryService
    {
        Task<SqlTier> GetTierAsync(int userId);
    }
}