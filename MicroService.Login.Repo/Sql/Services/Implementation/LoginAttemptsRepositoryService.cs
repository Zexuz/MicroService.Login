using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class LoginAttemptsRepositoryService : ILoginAttemptsRepositoryService
    {
        private readonly ILoginAttemptRepository _loginAttemptRepository;

        public LoginAttemptsRepositoryService(ILoginAttemptRepository loginAttemptRepository)
        {
            _loginAttemptRepository = loginAttemptRepository;
        }

        public async Task<LoginAttempt> AddLoginAttemptsAsync(LoginAttempt loginAttempt)
        {
            loginAttempt.Id = await _loginAttemptRepository.InsertAsync(loginAttempt.ToDatabase());
            return loginAttempt;
        }

        public async Task<List<LoginAttempt>> GetLoginAttempt(int userId)
        {
            var res = await _loginAttemptRepository.GetLoginAttempts(userId);
            return res.Select(item => item.FromDatabase()).ToList();
        }
    }
}