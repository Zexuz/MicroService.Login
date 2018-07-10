using System.Threading.Tasks;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface ITokenValidationService
    {
        Task<TokenValidationResult> IsIpWhitelistValid(string tokenString);
        Task<TokenValidationResult> IsEmailValidationValid(string tokenString);
        Task<TokenValidationResult> IsPasswordResetValid(string tokenString);
        Task<TokenValidationResult> IsAddTwoFactorAuthValid(string modelToken);
    }
}