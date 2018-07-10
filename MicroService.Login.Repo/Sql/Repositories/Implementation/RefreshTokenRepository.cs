using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class RefreshTokenRepository : SqlRepositoryBase<SqlRefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<SqlRefreshToken> FindAsync(int userId, string refreshTokenString)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                const string query = "SELECT * FROM RefreshToken WHERE UserId=UserId AND Value=@Value";
                return await cn.QueryFirstOrDefaultAsync<SqlRefreshToken>(query, new {UserId = userId, Value = refreshTokenString});
            }
        }

        public Task<int> RevokeAllRefreshTokensForUser(int userId)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                const string query = "UPDATE RefreshToken SET Valid=@valid WHERE UserId=UserId";
                return cn.ExecuteAsync(query, new {UserId = userId, Valid = false});
            }
        }
    }
}