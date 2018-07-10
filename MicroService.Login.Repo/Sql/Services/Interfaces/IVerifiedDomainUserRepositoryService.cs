using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface IVerifiedDomainUserRepositoryService
    {
        Task<VerifiedDomainUser>       InsertAsync(int resultUserId, string resultWebsite, SqlTransaction transaction);
        Task<List<VerifiedDomainUser>> FindAsync(List<int> userIds);
    }
}