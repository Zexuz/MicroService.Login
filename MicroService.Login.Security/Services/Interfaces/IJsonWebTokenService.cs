using System;
using System.Collections.Generic;
using System.Net;
using MicroService.Common.Core.Models;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Security.Services.Interfaces
{
    public interface IJsonWebTokenService
    {
        Token CreateLoginToken(User user, IPAddress ipAddress, string audience, string issuer, TimeSpan? lifespan = null);
        Token CreateToken(Dictionary<string, object> claims, TimeSpan lifespan);

        Token CreatePasswordResetToken(User user);
        Token CreateWhitelistIpValidationToken(User user, IPAddress address);
        Token CreateEmailValidationToken(User user);
        Token CreateTwoFactorToken(User user, string secret);

        T DecodeToken<T>(string token);
    }
}