using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using VerifiedDomainUser = MicroService.Login.Models.RepoService.VerifiedDomainUser;

namespace MicroService.Login.Repo.Sql.Services.Implementation
{
    public class VerifiedDomainUserRepositoryService : IVerifiedDomainUserRepositoryService
    {
        private readonly IVerifiedDomainUserRepository _repository;

        public VerifiedDomainUserRepositoryService(IVerifiedDomainUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<VerifiedDomainUser> InsertAsync(int ownerId, string websiteUrl, SqlTransaction transaction)
        {
            var vdUser = new Models.SqlVerifiedDomainUser
            {
                AllowDeposit = false,
                Created = DateTimeOffset.Now,
                OwnerId = ownerId,
                WebsiteUrl = websiteUrl,
                LastUpdated = DateTimeOffset.Now
            };
            vdUser.Id = await _repository.InsertAsync(vdUser, transaction);
            return vdUser.FromDatabase();
        }

        public async Task<List<VerifiedDomainUser>> FindAsync(List<int> userIds)
        {
            var res = await _repository.FindByIdsAsync(userIds);
            return res.Select(user => user.FromDatabase()).ToList();
        }
    }
}