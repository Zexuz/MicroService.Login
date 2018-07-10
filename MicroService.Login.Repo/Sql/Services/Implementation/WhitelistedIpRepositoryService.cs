using System;
using System.Net;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class WhitelistedIpRepositoryService : IWhitelistedIpRepositoryService
    {
        private readonly IWhitelistedIpRepository _whitelistedIpRepository;

        public WhitelistedIpRepositoryService(IWhitelistedIpRepository whitelistedIpRepository)
        {
            _whitelistedIpRepository = whitelistedIpRepository;
        }

        public async Task<WhitelistedIp> AddWhitelistedIpAsync(int userId, IPAddress ipAddress)
        {
            var whiteListedIp = new WhitelistedIp
            {
                Added = DateTimeOffset.Now,
                IpAddress = ipAddress,
                UserId = userId,
            };

            whiteListedIp.Id = await _whitelistedIpRepository.InsertAsync(whiteListedIp.ToDatabase());
            return whiteListedIp;
        }

        public async Task<bool> RemoveWhitelistedIpAsync(int userId, IPAddress ipAddress)
        {
            return await _whitelistedIpRepository.RemoveAsync(userId, ipAddress.ToString());
        }

        public async Task<bool> IsIpWhitelistedAsync(int userId, IPAddress ipAddress)
        {
            var res = await _whitelistedIpRepository.FindAsync(userId, ipAddress);
            return res != null;
        }
    }
}