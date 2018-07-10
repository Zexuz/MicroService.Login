using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Login.Models.ViewModel;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class UserLookupService : IUserLookupService
    {
        private readonly IUserRepositoryService               _userRepositoryService;
        private readonly IVerifiedDomainUserRepositoryService _domainUserRepositoryService;

        public UserLookupService(IUserRepositoryService userRepositoryService, IVerifiedDomainUserRepositoryService domainUserRepositoryService)
        {
            _userRepositoryService = userRepositoryService;
            _domainUserRepositoryService = domainUserRepositoryService;
        }

        public async Task<List<UserViewModel>> FindAsync(List<int> userIds)
        {
            var users = await _userRepositoryService.FindAsync(userIds);

            var domainVerifiedUserIds = users.Where(user => user.DomainId != null).Select(user => user.Id).ToList();

            var domainVerifiedUsers = await _domainUserRepositoryService.FindAsync(domainVerifiedUserIds);

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var item = new UserViewModel
                {
                    Id = user.Id,
                    MemberSince = user.Created,
                    Username = user.Username,
                };

                if (user.DomainId != null)
                {
                    var verifiedDomainUser = domainVerifiedUsers.First(domainUser => domainUser.OwnerId == user.Id);
                    item.VerifiedDomain = new VerifiedDomainUserViewModel
                    {
                        AllowDeposit = verifiedDomainUser.AllowDeposit,
                        Created = verifiedDomainUser.Created,
                        Description = verifiedDomainUser.Description,
                        LastUpdated = verifiedDomainUser.LastUpdated,
                        Website = new Uri(verifiedDomainUser.WebsiteUrl)
                    };
                }

                userViewModels.Add(item);
            }


            return userViewModels;
        }
    }
}