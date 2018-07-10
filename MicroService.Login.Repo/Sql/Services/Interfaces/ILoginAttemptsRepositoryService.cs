using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface ILoginAttemptsRepositoryService
    {
        Task<LoginAttempt>       AddLoginAttemptsAsync(LoginAttempt loginAttempt);
        Task<List<LoginAttempt>> GetLoginAttempt(int userId);
    }
}