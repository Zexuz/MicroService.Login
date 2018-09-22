using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Login.Models.ViewModel;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class UserLookupService : IUserLookupService
    {
        private readonly IUserRepositoryService               _userRepositoryService;

        public UserLookupService(IUserRepositoryService userRepositoryService)
        {
            _userRepositoryService = userRepositoryService;
        }

        public async Task<List<UserViewModel>> FindAsync(List<int> userIds)
        {
            var users = await _userRepositoryService.FindAsync(userIds);

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var item = new UserViewModel
                {
                    Id = user.Id,
                    MemberSince = user.Created,
                    Username = user.Username,
                };

                userViewModels.Add(item);
            }

            return userViewModels;
        }
    }
}