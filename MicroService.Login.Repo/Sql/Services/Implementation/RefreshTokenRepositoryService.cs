using System;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class RefreshTokenRepositoryService : IRefreshTokenRepositoryService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenRepositoryService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<RefreshToken> AddRefreshToken(RefreshToken refreshToken)
        {
            refreshToken.Id = await _refreshTokenRepository.InsertAsync(refreshToken.ToDatabase());
            return refreshToken;
        }

        public async Task<bool> RevokeRefreshToken(int userId, string refreshToken)
        {
            var token = await _refreshTokenRepository.FindAsync(userId, refreshToken);
            if (token == null)
                return false;

            if (!token.Valid)
                return true;

            token.Valid = false;
            token.Revoked = DateTimeOffset.Now;
            await _refreshTokenRepository.UpdateAsync(token);
            return true;
        }

        public async Task<bool> UpdateRefreshToken(RefreshToken refreshToken)
        {
            return await _refreshTokenRepository.UpdateAsync(refreshToken.ToDatabase());
        }

        public async Task<RefreshToken> GetIssuedRefreshToken(int userId, string refreshTokenString)
        {
            var res = await _refreshTokenRepository.FindAsync(userId, refreshTokenString);
            return res.FromDatabase();
        }

        public async Task<int> RevokeAllRefreshTokens(int userId)
        {
            return await _refreshTokenRepository.RevokeAllRefreshTokensForUser(userId);
        }
    }
}