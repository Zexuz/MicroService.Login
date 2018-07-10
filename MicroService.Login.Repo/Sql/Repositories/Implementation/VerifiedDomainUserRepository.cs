using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class VerifiedDomainUserRepository : SqlRepositoryBase<SqlVerifiedDomainUser>, IVerifiedDomainUserRepository
    {
        public VerifiedDomainUserRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<IEnumerable<SqlVerifiedDomainUser>> FindByIdsAsync(List<int> userIds)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                return await cn.QueryAsync<SqlVerifiedDomainUser>("SELECT * FROM [VerifiedDomainUser] WHERE OwnerId IN @ids", new {Ids = userIds});
            }
        }
    }
}