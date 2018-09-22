using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class UserRepositoryService : IUserRepositoryService
    {
        private readonly IUserRepository _userRepository;

        public UserRepositoryService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string hashedPassword)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            user.Password = hashedPassword;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<int> InsertAsync(User user)
        {
            return await _userRepository.InsertAsync(user.ToDatabase());
        }

        public async Task<User> FindAsync(string username)
        {
            var res = await _userRepository.FindByUsernameAsync(username);
            return res.FromDatabase();
        }

        public async Task<User> FindAsync(int id)
        {
            var res = await _userRepository.FindByIdAsync(id);
            return res.FromDatabase();
        }

        public async Task<User> FindAsync(int id, IDbTransaction transaction)
        {
            var res = await _userRepository.FindByIdAsync(id, transaction);
            return res.FromDatabase();
        }

        public async Task<User> FindAsync(Email email)
        {
            var res = await _userRepository.FindByEmailAsync(email.Value);
            return res.FromDatabase();
        }

        public async Task<List<User>> FindAsync(List<int> userIds)
        {
            var res = await _userRepository.FindByIdsAsync(userIds);
            return res.Select(user => user.FromDatabase()).ToList();
        }

        public async Task<bool> EnableTwoFactorForUserAsync(int userId, string secret)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            user.TwoFactorSecret = secret;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DisableTwoFactorForUserAsync(int userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            user.TwoFactorSecret = null;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> ChangeEmailVerifiedStatus(int userId, bool status)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            user.EmailVerified = status;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> HasVerifiedEmailAsync(int userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            return user.EmailVerified;
        }
    }
}