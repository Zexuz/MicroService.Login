using System.Net;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(User user, string baseUrl);
        Task<bool> SendWhitelistIpEmailAsync(IPAddress connectionIp, User user, string baseUrl);
        Task<bool> SendValidationEmailAsync(User user, string baseUrl);

        void SendResetPasswordEmail(User user, string baseUrl);
        void SendWhitelistIpEmail(IPAddress connectionIp, User user, string baseUrl);
        void SendValidationEmail(User user, string baseUrl);
    }
}