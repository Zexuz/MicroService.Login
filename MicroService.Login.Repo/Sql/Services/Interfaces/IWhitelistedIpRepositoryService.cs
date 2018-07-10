using System.Net;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface IWhitelistedIpRepositoryService
    {
        Task<WhitelistedIp> AddWhitelistedIpAsync(int userId, IPAddress ipAddress);
        Task<bool>          RemoveWhitelistedIpAsync(int userId, IPAddress ipAddress);
        Task<bool>          IsIpWhitelistedAsync(int userId, IPAddress ipAddress);
    }
}