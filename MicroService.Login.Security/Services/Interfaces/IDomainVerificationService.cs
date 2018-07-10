using System.Threading.Tasks;
using MicroService.Login.Repo.MongoDb.Models;
using MicroService.Login.Security.Services.Impl;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IDomainVerificationService
    {
        Task<VerifyDomainRequest> Init(int userId, string website);
        Task<VerifyDomainResult>  VerifyDomain(int userId);
        Task<bool>                DeletePendingAsync(int parse);
        Task<DomainScraper>       GetPendingAsync(int userId);
    }
}