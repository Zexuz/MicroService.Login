using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : ISqlRepositoryBase<SqlRefreshToken>
    {
        Task<SqlRefreshToken> FindAsync(int userId, string refreshTokenString);
        Task<int>             RevokeAllRefreshTokensForUser(int userId);
    }
}