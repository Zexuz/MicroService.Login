using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface IRefreshTokenRepositoryService
    {
        Task<RefreshToken> AddRefreshToken(RefreshToken refreshToken);
        Task<bool>         RevokeRefreshToken(int userId, string refreshToken);
        Task<bool>         UpdateRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> GetIssuedRefreshToken(int userId, string refreshTokenString);
        Task<int>          RevokeAllRefreshTokens(int userId);
    }
}