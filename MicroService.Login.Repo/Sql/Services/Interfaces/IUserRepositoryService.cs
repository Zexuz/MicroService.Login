using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo.Sql.Services.Interfaces
{
    public interface IUserRepositoryService
    {
        Task<bool>       UpdateUserPasswordAsync(int userId, string hashedPassword);
        Task<int>        InsertAsync(User user);
        Task<User>       FindAsync(string username);
        Task<User>       FindAsync(int id);
        Task<User>       FindAsync(int id, IDbTransaction transaction);
        Task<User>       FindAsync(Email email);
        Task<List<User>> FindAsync(List<int> userIds);
        Task<bool>       EnableTwoFactorForUserAsync(int userId, string secret);
        Task<bool>       DisableTwoFactorForUserAsync(int userId);
        Task<bool>       ChangeEmailVerifiedStatus(int userId, bool status);
        Task<bool>       HasVerifiedEmailAsync(int userId);
    }
}