using MicroService.Login.Models;

namespace MicroService.Login.Security.Helpers
{
    public static class RoleParser
    {
        public static int ToInt(Role role)
        {
            return (int) role;
        }

        public static Role FromInt(int role)
        {
            return (Role) role;
        }
    }
}