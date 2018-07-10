using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface ITierRepository : ISqlRepositoryBase<SqlTier>
    {
    }
}