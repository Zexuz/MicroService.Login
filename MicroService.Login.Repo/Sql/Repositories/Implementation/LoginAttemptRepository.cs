using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Tables;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class LoginAttemptRepository : SqlRepositoryBase<SqlLoginAttempt>, ILoginAttemptRepository
    {
        public LoginAttemptRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<List<SqlLoginAttempt>> GetLoginAttempts(int userId)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                var query = $"SELECT * FROM {new LoginAttemtTable().TableName} WHERE UserId=@userid";
                return (await cn.QueryAsync<SqlLoginAttempt>(query, new {UserId = userId})).ToList();
            }
        }
    }
}