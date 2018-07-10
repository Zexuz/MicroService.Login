using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class TierRepository : SqlRepositoryBase<SqlTier>, ITierRepository
    {
        public TierRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }
    }
}