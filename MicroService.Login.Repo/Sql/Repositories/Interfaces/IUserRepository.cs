using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;

namespace MicroService.Login.Repo.Sql.Repositories.Interfaces
{
    public interface IUserRepository : ISqlRepositoryBase<SqlUser>
    {
        Task<SqlUser>              FindByEmailAsync(string email);
        Task<SqlUser>              FindByUsernameAsync(string username);
        Task<IEnumerable<SqlUser>> FindByIdsAsync(List<int> userIds);
    }
}