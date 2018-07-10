using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JWT;
using MicroService.Common.Core.Exceptions;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Misc;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Common.Core.Web;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Models.Resources;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Models;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IUserRepositoryService          _userRepositoryService;
        private readonly IPasswordHashService            _passwordHashService;
        private readonly IJsonWebTokenService            _jwtService;
        private readonly IHttpRequestParser              _httpRequestParser;
        private readonly IEmailService                   _emailService;
        private readonly ITwoFactorAuthenticatorManager  _factorAuthenticatorManager;
        private readonly IWhitelistedIpRepositoryService _whitelistedIpRepositoryService;
        private readonly ILoginAttemptsRepositoryService _loginAttemptsRepositoryService;
        private readonly IRefreshTokenRepositoryService  _refreshTokenRepositoryService;

        public UserService
        (
            IUserRepositoryService userRepositoryService,
            IPasswordHashService passwordHashService,
            IJsonWebTokenService jwtService,
            IHttpRequestParser httpRequestParser,
            IEmailService emailService,
            ITwoFactorAuthenticatorManager factorAuthenticatorManager,
            IWhitelistedIpRepositoryService whitelistedIpRepositoryService,
            ILoginAttemptsRepositoryService loginAttemptsRepositoryService,
            IRefreshTokenRepositoryService refreshTokenRepositoryService
        )
        {
            _userRepositoryService = userRepositoryService;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
            _httpRequestParser = httpRequestParser;
            _emailService = emailService;
            _factorAuthenticatorManager = factorAuthenticatorManager;
            _whitelistedIpRepositoryService = whitelistedIpRepositoryService;
            _loginAttemptsRepositoryService = loginAttemptsRepositoryService;
            _refreshTokenRepositoryService = refreshTokenRepositoryService;
        }

        public async Task<User> FindAsync(Email email)
        {
            return await _userRepositoryService.FindAsync(email);
        }

        public async Task<User> FindAsync(int id)
        {
            return await _userRepositoryService.FindAsync(id);
        }

        public async Task<User> FindAsync(string username)
        {
            return await _userRepositoryService.FindAsync(username);
        }

        public async Task<CreateUserReponse> CreateUserAsync(User user, IPAddress ipAddress)
        {
            if (await IsEmailTakenAsync(user.Email))
                return new CreateUserReponse(false, string.Format(StrResource.EmailTaken, user.Email.Value));

            if (await IsUsernameTakenAsync(user.Username))
                return new CreateUserReponse(false, string.Format(StrResource.UsernameTaken, user.Username));

            user.Password = _passwordHashService.GenerateHashFromPassword(user.Password);
            user.Id = await _userRepositoryService.InsertAsync(user);


            await WhitelistIpAsync(user, ipAddress);
            _emailService.SendValidationEmail(user, "someUrl");

            return new CreateUserReponse(true);
        }

        public async Task UpdateEmailValidationStatusAsync(int userId, bool status = true)
        {
            await _userRepositoryService.ChangeEmailVerifiedStatus(userId, status);
        }

        public async Task<LoginResponse> LoginUser(Email email, string plainPassword, string twoFactorCode, ConnectionInfo connectionInfo)
        {
            var user = await FindAsync(email);

            if (user == null)
                return new LoginResponse
                {
                    Error = LoginError.AccountNotFound,
                    Success = false
                };

            var loginResponse = await ValidateUser(user, plainPassword, connectionInfo.IpAddress, twoFactorCode);

            await AddLoginAttemptAsync(loginResponse.Error, user.Id, connectionInfo);

            if (!loginResponse.Success)
            {
                if (loginResponse.Error == LoginError.NotWhitelistedIp)
                    _emailService.SendWhitelistIpEmail(connectionInfo.IpAddress, user, "someBaseUrl");
                return loginResponse;
            }

            var refreshToken = await IssueRefreshToken(user, connectionInfo);

            loginResponse.OAuthToken = new OAuthToken
            {
                AccessToken = refreshToken.AccessToken.TokenString,
                Expires = refreshToken.AccessToken.Expires,
                RefreshToken = refreshToken.RefreshTokenString,
                Type = OAuthToken.OAuthTokenType.Bearer
            };

            return loginResponse;
        }

        public async Task<bool> ResetPasswordAsync(int id, string newPlainPassword)
        {
            var user = await FindAsync(id);
            if (user == null) return false;
            return await ResetPasswordAsync(user, newPlainPassword);
        }

        public async Task<bool> ResetPasswordAsync(User user, string newPlainPassword)
        {
            var hashedPassword = _passwordHashService.GenerateHashFromPassword(newPlainPassword);
            var passwordResetResult = await _userRepositoryService.UpdateUserPasswordAsync(user.Id, hashedPassword);

            if (!passwordResetResult)
                return false;

            await _refreshTokenRepositoryService.RevokeAllRefreshTokens(user.Id);

            return true;
        }

        public async Task<int> RevokeAllRefreshTokens(int userId)
        {
            return await _refreshTokenRepositoryService.RevokeAllRefreshTokens(userId);
        }

        public Task<bool> HasVerfifiedEmailAsync(User user)
        {
            return _userRepositoryService.HasVerifiedEmailAsync(user.Id);
        }

        public async Task<Token> GenreateNewAccessToken(User user, string refreshTokenString)
        {
            var refreshToken = await _refreshTokenRepositoryService.GetIssuedRefreshToken(user.Id, refreshTokenString);
            if (refreshToken == null)
                throw new RefreshTokenDoesNotExist();

            if (!refreshToken.Valid)
                throw new TokenExpiredException("The refresh token has been revoked");

            var accessToken = _jwtService.CreateLoginToken(user, IPAddress.Parse(refreshToken.FromIp), "localhost", "localhost");

            refreshToken.LastUsed = DateTimeOffset.Now;

            await _refreshTokenRepositoryService.UpdateRefreshToken(refreshToken);
            return accessToken;
        }

        public async Task<bool> EnableTwoFactorForUser(User user, string secret)
        {
            return await _userRepositoryService.EnableTwoFactorForUserAsync(user.Id, secret);
        }

        public async Task<bool> DisableTwoFactorForUser(User user)
        {
            return await _userRepositoryService.DisableTwoFactorForUserAsync(user.Id);
        }

        public async Task<bool> RevokeRefreshToken(int userId, string refreshToken)
        {
            return await _refreshTokenRepositoryService.RevokeRefreshToken(userId, refreshToken);
        }

        public async Task<bool> RemoveWhitelistedIpAsync(int userId, IPAddress ipAddress)
        {
            return await _whitelistedIpRepositoryService.RemoveWhitelistedIpAsync(userId, ipAddress);
        }

        public async Task WhitelistIpAsync(User user, IPAddress address)
        {
            await WhitelistIpAsync(user.Id, address);
        }

        public async Task WhitelistIpAsync(int id, IPAddress address)
        {
            if (!await _whitelistedIpRepositoryService.IsIpWhitelistedAsync(id, address))
                await _whitelistedIpRepositoryService.AddWhitelistedIpAsync(id, address);
        }

        private async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await FindAsync(username) != null;
        }

        private async Task<bool> IsEmailTakenAsync(Email email)
        {
            return await FindAsync(email) != null;
        }

        private async Task<GeneratedToken> IssueRefreshToken(User user, ConnectionInfo connectionInfo)
        {
            var token = _jwtService.CreateLoginToken(user, connectionInfo.IpAddress, "localhost", "localhost");
            var refreshToken = await GenerateRefreshToken(connectionInfo, user.Id);

            await _refreshTokenRepositoryService.AddRefreshToken(refreshToken);
            return new GeneratedToken
            {
                AccessToken = token,
                RefreshTokenString = refreshToken.Value
            };
        }

        private async Task<RefreshToken> GenerateRefreshToken(ConnectionInfo connectionInfo, int userId)
        {
            var requestMessage = new RequestMessage(HttpMethod.Get, $"https://ipapi.co/{connectionInfo.IpAddress}/json");
            var ipLookupTask = await _httpRequestParser.ExecuteAsType<IpLookupResource>(requestMessage);

            return new RefreshToken
            {
                CountryCode = ipLookupTask.Country ?? "Unknow",
                FromIp = connectionInfo.IpAddress.ToString(),
                Created = DateTimeOffset.Now,
                Valid = true,
                Value = GenerateSecureString(),
                UserId = userId,
                LastUsed = DateTimeOffset.Now,
                Revoked = null,
                Browser = connectionInfo.BrowserInfo.BrowserFamily,
                Device = connectionInfo.BrowserInfo.DeviceFamily,
                Os = connectionInfo.BrowserInfo.OsFamily
            };
        }

        private async Task AddLoginAttemptAsync(LoginError loginError, int userId, ConnectionInfo connectionInfo)
        {
            var loginAttempt = new LoginAttempt
            {
                FromIp = connectionInfo.IpAddress.ToString(),
                Date = DateTimeOffset.Now,
                Success = loginError == LoginError.None,
                Reason = loginError.ToString(),
                UserId = userId,
                Browser = connectionInfo.BrowserInfo.BrowserFamily,
                Device = connectionInfo.BrowserInfo.DeviceFamily,
                Os = connectionInfo.BrowserInfo.OsFamily
            };

            await _loginAttemptsRepositoryService.AddLoginAttemptsAsync(loginAttempt);
        }

        private async Task<LoginResponse> ValidateUser(User user, string plainPassword, IPAddress address, string twoFactorCode)
        {
            if (!_passwordHashService.IsValidHash(plainPassword, user.Password))
                return new LoginResponse
                {
                    Error = LoginError.InvalidLoginDetails,
                    Success = false
                };

            if (!await _userRepositoryService.HasVerifiedEmailAsync(user.Id))
                return new LoginResponse
                {
                    Error = LoginError.NotValidatedEmail,
                    Success = false
                };

            if (!await _whitelistedIpRepositoryService.IsIpWhitelistedAsync(user.Id, address))
                return new LoginResponse
                {
                    Error = LoginError.NotWhitelistedIp,
                    Success = false
                };

            if (user.TwoFactorSecret != null && string.IsNullOrEmpty(twoFactorCode))
                return new LoginResponse
                {
                    Error = LoginError.TwoFactorRequiered,
                    Success = false
                };

            if (user.TwoFactorSecret != null && !_factorAuthenticatorManager.VerifyCode(twoFactorCode, user.TwoFactorSecret, user.Email.Value))
                return new LoginResponse
                {
                    Error = LoginError.TwoFactorInvalid,
                    Success = false
                };


            return new LoginResponse
            {
                Error = LoginError.None,
                Success = true,
            };
        }

        private string GenerateSecureString(int i = 64)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[i];
            provider.GetBytes(byteArray);

            var randomString = Convert.ToBase64String(byteArray, 0);

            return randomString;
        }

        private struct GeneratedToken
        {
            public Token  AccessToken        { get; set; }
            public string RefreshTokenString { get; set; }
        }
    }
}