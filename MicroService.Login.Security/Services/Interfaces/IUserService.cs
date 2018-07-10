using System.Net;
using System.Threading.Tasks;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Security.Models;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> FindAsync(Email email);
        Task<User> FindAsync(int id);
        Task<User> FindAsync(string username);

        Task<CreateUserReponse> CreateUserAsync(User user, IPAddress ipAddress);

        Task<LoginResponse> LoginUser(Email email, string plainPassword, string twoFactorCode, ConnectionInfo connectionInfo);

        Task UpdateEmailValidationStatusAsync(int userId, bool status = true);

        Task WhitelistIpAsync(int id, IPAddress address);
        Task WhitelistIpAsync(User user, IPAddress address);

        Task<bool> ResetPasswordAsync(int id, string newPlainPassword);
        Task<bool> ResetPasswordAsync(User user, string newPlainPassword);
        Task<int>  RevokeAllRefreshTokens(int userId);

        Task<bool>  HasVerfifiedEmailAsync(User user);
        Task<Token> GenreateNewAccessToken(User user, string refreshTokenString);
        Task<bool>  EnableTwoFactorForUser(User user, string secret);
        Task<bool>  DisableTwoFactorForUser(User user);
        Task<bool>  RevokeRefreshToken(int userId, string refreshToken);
        Task<bool>  RemoveWhitelistedIpAsync(int parse, IPAddress ipAddress);
    }
}