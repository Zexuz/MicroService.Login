using System.Net;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface IWhitelistedIpRepository : ISqlRepositoryBase<SqlWhitelistedIp>
    {
        Task<bool>             RemoveAsync(int userId, string ipAddress);
        Task<SqlWhitelistedIp> FindAsync(int userId, IPAddress ipAddress);
    }
}