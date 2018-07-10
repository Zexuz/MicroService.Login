using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface IVerifiedDomainUserRepository : ISqlRepositoryBase<SqlVerifiedDomainUser>
    {
        Task<IEnumerable<SqlVerifiedDomainUser>> FindByIdsAsync(List<int> userIds);
    }
}