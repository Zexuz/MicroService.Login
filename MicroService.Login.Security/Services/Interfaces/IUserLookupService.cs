using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Login.Models.ViewModel;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IUserLookupService
    {
        Task<List<UserViewModel>> FindAsync(List<int> userIds);
    }
}