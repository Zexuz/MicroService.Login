using MicroService.Common.Core.Models;

namespace MicroService.Login.Security
{
    public class LoginResponse
    {
        public bool       Success    { get; set; }
        public LoginError Error      { get; set; }
        public OAuthToken OAuthToken { get; set; }
    }
}