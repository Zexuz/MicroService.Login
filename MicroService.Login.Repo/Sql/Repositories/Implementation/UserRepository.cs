using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class UserRepository : SqlRepositoryBase<SqlUser>, IUserRepository
    {
        public UserRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }


        public async Task<SqlUser> FindByEmailAsync(string email)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                return await cn.QuerySingleOrDefaultAsync<SqlUser>("SELECT * FROM [User] WHERE Email=@Email", new {Email = email});
            }
        }

        public async Task<SqlUser> FindByUsernameAsync(string username)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                return await cn.QuerySingleOrDefaultAsync<SqlUser>("SELECT * FROM [User] WHERE Username=@Username", new {Username = username});
            }
        }

        public async Task<IEnumerable<SqlUser>> FindByIdsAsync(List<int> userIds)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                return await cn.QueryAsync<SqlUser>("SELECT * FROM [User] WHERE Id IN @ids", new {Ids = userIds});
            }
        }
    }
}