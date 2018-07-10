using System;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using JWT;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Misc;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Common.Core.Web;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Models.Resources;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security;
using MicroService.Login.Security.Services.Impl;
using MicroService.Login.Security.Services.Interfaces;
using MicroService.Login.WebApi.Models;
using Xunit;

namespace MicroService.Login.Webapi.Test
{
    public class UserServiceTest
    {
        private readonly IUserRepositoryService          _userRepositoryService;
        private readonly UserService                     _userService;
        private readonly IPasswordHashService            _passwordHash;
        private readonly IJsonWebTokenService            _jwtService;
        private readonly IHttpRequestParser              _httpRequestParser;
        private readonly IEmailService                   _emailService;
        private readonly ITwoFactorAuthenticatorManager  _factorAuthenticatorManager;
        private readonly ILoginAttemptsRepositoryService _loginAttemptRepoService;
        private readonly IRefreshTokenRepositoryService  _refreshTokenRepositoryService;
        private readonly IWhitelistedIpRepositoryService _whiteListRepostirotyService;

        public UserServiceTest()
        {
            _passwordHash = A.Fake<IPasswordHashService>();
            _userRepositoryService = A.Fake<IUserRepositoryService>();
            _jwtService = A.Fake<IJsonWebTokenService>();
            _httpRequestParser = A.Fake<IHttpRequestParser>();
            _emailService = A.Fake<IEmailService>();
            _factorAuthenticatorManager = A.Fake<ITwoFactorAuthenticatorManager>();
            _loginAttemptRepoService = A.Fake<ILoginAttemptsRepositoryService>();
            _refreshTokenRepositoryService = A.Fake<IRefreshTokenRepositoryService>();
            _whiteListRepostirotyService = A.Fake<IWhitelistedIpRepositoryService>();

            _userService = new UserService
            (
                _userRepositoryService,
                _passwordHash,
                _jwtService,
                _httpRequestParser,
                _emailService,
                _factorAuthenticatorManager,
                _whiteListRepostirotyService,
                _loginAttemptRepoService,
                _refreshTokenRepositoryService
            );
        }

        [Fact]
        public async Task CreateUserSuccess()
        {
            var ip = IPAddress.Parse("80.80.80.45");
            A.CallTo(() => _passwordHash.GenerateHashFromPassword("My password")).Returns("hashedPassword");

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => _userRepositoryService.FindAsync(A<string>._)).Returns(Task.FromResult<User>(null));

            A.CallTo(() => _userRepositoryService.InsertAsync(A<User>._)).Returns(1);

            var response = await _userService.CreateUserAsync(new User
            {
                Email = new Email("email1@email.com"),
                Password = "My password",
                Username = "Username"
            }, ip);

            A.CallTo(() => _userRepositoryService.InsertAsync(
                A<User>.That.Matches(user =>
                    user.Email.Value == "email1@email.com" &&
                    user.Password    == "hashedPassword"   &&
                    user.Username    == "Username"
                )
            )).MustHaveHappened();

            A.CallTo(() => _whiteListRepostirotyService.AddWhitelistedIpAsync
            (
                A<int>.That.Matches(id => id                == 1),
                A<IPAddress>.That.Matches(i => i.ToString() == ip.ToString())
            )).MustHaveHappened();

            A.CallTo(() => _emailService.SendValidationEmail(A<User>.That.Matches(u => u.Id == 1), A<string>._)).MustHaveHappened();
            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>.That.Matches(e => e.Value == "email1@email.com"))).MustHaveHappened();
            A.CallTo(() => _userRepositoryService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _passwordHash.GenerateHashFromPassword("My password")).MustHaveHappened();
            Assert.True(response.Success);
        }

        [Fact]
        public async Task CrateUserFailsEmailAlreadyTaken()
        {
            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User());

            var response = await _userService.CreateUserAsync(new User
            {
                Email = new Email("email@email.com"),
                Password = "My password",
                Username = "Username"
            }, IPAddress.Any);

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>.That.Matches(e => e.Value == "email@email.com"))).MustHaveHappened();
            A.CallTo(() => _passwordHash.GenerateHashFromPassword(A<string>._)).MustNotHaveHappened();

            Assert.False(response.Success);
            Assert.Equal(string.Format(StrResource.EmailTaken, "email@email.com"), response.Error);
        }

        [Fact]
        public async Task CrateUserFailsUsernameAlreadyTaken()
        {
            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns<User>(null);
            A.CallTo(() => _userRepositoryService.FindAsync(A<string>._)).Returns(new User());

            var response = await _userService.CreateUserAsync(new User
            {
                Email = new Email("email12@email.com"),
                Password = "My password",
                Username = "Username"
            }, IPAddress.Any);


            A.CallTo(() => _userRepositoryService.FindAsync(A<string>._)).MustHaveHappened();
            A.CallTo(() => _passwordHash.GenerateHashFromPassword(A<string>._)).MustNotHaveHappened();

            Assert.False(response.Success);
            Assert.Equal(string.Format(StrResource.UsernameTaken, "Username"), response.Error);
        }

        [Fact]
        public async Task UserLoginSuccess()
        {
            var email = new Email("s@s.c");
            var password = "password";
            var ipAddress = IPAddress.Parse("1.1.1.1"); //yeee, cloudflare dns
            var twoFaCode = "valid";

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User {Email = email, TwoFactorSecret = "secret"});
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).Returns(true);
            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, ipAddress)).Returns(true);
            A.CallTo(() => _factorAuthenticatorManager.VerifyCode(A<string>._, A<string>._, A<string>._)).Returns(true);

            var loginResult = await _userService.LoginUser(email, password, twoFaCode,
                new ConnectionInfo {IpAddress = ipAddress, BrowserInfo = new BrowserInfo()});

            Assert.True(loginResult.Success);
            Assert.Equal(LoginError.None, loginResult.Error);

            A.CallTo(() => _httpRequestParser.ExecuteAsType<IpLookupResource>(A<RequestMessage>._)).MustHaveHappened();
            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();
            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, ipAddress)).MustHaveHappened();
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).MustHaveHappened();
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).MustHaveHappened();
            A.CallTo(() => _refreshTokenRepositoryService.AddRefreshToken(A<RefreshToken>._)).MustHaveHappened();
            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, A<TimeSpan?>._)).MustHaveHappened();
            A.CallTo(() => _factorAuthenticatorManager.VerifyCode(twoFaCode, "secret", email.Value)).MustHaveHappened();
        }

        [Fact]
        public async Task UserLoginNoneExistingEmailError()
        {
            var email = new Email("s@s.c");
            var password = "password";
            var ipAddress = IPAddress.Parse("1.1.1.1"); //yeee, cloudflare dns

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns<User>(null);

            var loginResult = await _userService.LoginUser(email, password, null, new ConnectionInfo {IpAddress = ipAddress});

            Assert.False(loginResult.Success);
            Assert.Equal(LoginError.AccountNotFound, loginResult.Error);
            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, A<TimeSpan?>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task UserLoginInvalidPasswordError()
        {
            var email = new Email("s@s.c");
            var password = "password";
            var ipAddress = IPAddress.Parse("1.1.1.1"); //yeee, cloudflare dns

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User());
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).Returns(false);

            var loginResult = await _userService.LoginUser(email, password, null,
                new ConnectionInfo {IpAddress = ipAddress, BrowserInfo = new BrowserInfo()});

            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();

            Assert.False(loginResult.Success);
            Assert.Equal(LoginError.InvalidLoginDetails, loginResult.Error);
            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, A<TimeSpan?>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task UserLoginNotVerifiedEmailError()
        {
            var email = new Email("s@s.c");
            var password = "password";
            var ipAddress = IPAddress.Parse("1.1.1.1"); //yeee, cloudflare dns

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User());
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).Returns(false);

            var loginResult = await _userService.LoginUser(email, password, null,
                new ConnectionInfo {IpAddress = ipAddress, BrowserInfo = new BrowserInfo()});


            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).MustHaveHappened();
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).MustHaveHappened();
            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();
            Assert.False(loginResult.Success);
            Assert.Equal(LoginError.NotValidatedEmail, loginResult.Error);
            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, A<TimeSpan?>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task UserLoginNotWhiteListedIp()
        {
            var email = new Email("s@s.c");
            var password = "password";
            var ipAddress = IPAddress.Parse("1.1.1.1"); //yeee, cloudflare dns

            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User());
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).Returns(true);

            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, ipAddress)).Returns(false);

            var loginResult = await _userService.LoginUser(email, password, null,
                new ConnectionInfo {IpAddress = ipAddress, BrowserInfo = new BrowserInfo()});

            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, ipAddress)).MustHaveHappened();
            A.CallTo(() => _passwordHash.IsValidHash(password, A<string>._)).MustHaveHappened();
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).MustHaveHappened();
            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();
            A.CallTo(() => _emailService.SendWhitelistIpEmail(ipAddress, A<User>._, A<string>._)).MustHaveHappened();

            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, A<TimeSpan?>._)).MustNotHaveHappened();

            Assert.False(loginResult.Success);
            Assert.Equal(LoginError.NotWhitelistedIp, loginResult.Error);
        }


        [Fact]
        public async Task UserLoignFailsNoProvied2Fa()
        {
            var loginModel = new LoginModel
            {
                Email = "myEmail@valid.com",
                Password = "MyPassword"
            };

            A.CallTo(() => _passwordHash.IsValidHash(A<string>._, A<string>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).Returns(true);

            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, A<IPAddress>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User
            {
                TwoFactorSecret = "secret"
            });


            var result = await _userService.LoginUser(new Email(loginModel.Email), loginModel.Password, loginModel.TwoFactorCode,
                new ConnectionInfo {IpAddress = IPAddress.Any, BrowserInfo = new BrowserInfo()});

            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();

            Assert.Equal(LoginError.TwoFactorRequiered, result.Error);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UserLoignFailsInvalid2Fa()
        {
            var loginModel = new LoginModel
            {
                Email = "myEmail@valid.com",
                Password = "MyPassword",
                TwoFactorCode = "invalid"
            };

            A.CallTo(() => _passwordHash.IsValidHash(A<string>._, A<string>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.HasVerifiedEmailAsync(A<int>._)).Returns(true);
            A.CallTo(() => _whiteListRepostirotyService.IsIpWhitelistedAsync(A<int>._, A<IPAddress>._)).Returns(true);
            A.CallTo(() => _userRepositoryService.FindAsync(A<Email>._)).Returns(new User
            {
                Email = new Email(loginModel.Email),
                TwoFactorSecret = "secret"
            });
            A.CallTo(() => _factorAuthenticatorManager.VerifyCode(A<string>._, A<string>._, A<string>._)).Returns(false);

            var result = await _userService.LoginUser(new Email(loginModel.Email), loginModel.Password, loginModel.TwoFactorCode,
                new ConnectionInfo {IpAddress = IPAddress.Any, BrowserInfo = new BrowserInfo()});

            A.CallTo(() => _loginAttemptRepoService.AddLoginAttemptsAsync(A<LoginAttempt>._)).MustHaveHappened();
            A.CallTo(() => _factorAuthenticatorManager.VerifyCode("invalid", "secret", "myEmail@valid.com")).MustHaveHappened();

            Assert.Equal(LoginError.TwoFactorInvalid, result.Error);
            Assert.False(result.Success);
        }


        [Fact]
        public async Task UserRequestNewAccessTokenWithValidRefreshTokenSuccess()
        {
            A.CallTo(() => _refreshTokenRepositoryService.GetIssuedRefreshToken(A<int>._, A<string>._)).Returns(new RefreshToken
            {
                FromIp = "1.1.1.1",
                Created = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                LastUsed = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                Revoked = null,
                Valid = true,
                Value = "someString"
            });

            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, null)).Returns(new Token());

            await _userService.GenreateNewAccessToken(new User(), "someString");

            A.CallTo(() => _refreshTokenRepositoryService.UpdateRefreshToken(A<RefreshToken>._)).MustHaveHappened();
        }

        [Fact]
        public async Task UserRequestNewAccessTokenWithInValidRefreshTokenError()
        {
            A.CallTo(() => _refreshTokenRepositoryService.GetIssuedRefreshToken(A<int>._, A<string>._)).Returns(new RefreshToken
            {
                FromIp = "1.1.1.1",
                Created = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                LastUsed = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                Revoked = null,
                Valid = false,
                Value = "someString"
            });

            await Assert.ThrowsAnyAsync<TokenExpiredException>(async () => await _userService.GenreateNewAccessToken(new User(), "someString"));

            A.CallTo(() => _jwtService.CreateLoginToken(A<User>._, A<IPAddress>._, A<string>._, A<string>._, null)).MustNotHaveHappened();
            A.CallTo(() => _refreshTokenRepositoryService.UpdateRefreshToken(A<RefreshToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task UserLogoutSuccess()
        {
            await _userService.RevokeRefreshToken(1, "someString");
            A.CallTo(() => _refreshTokenRepositoryService.RevokeRefreshToken(A<int>._, "someString")).MustHaveHappened();
        }

        [Fact]
        public async void PasswordResetInvalidatesAllRefreshTokens()
        {
            var userId = 1;
            var newPlainPassword = "plaintPassword";

            A.CallTo(() => _userRepositoryService.FindAsync(userId)).Returns(new User {Id = userId});
            A.CallTo(() => _userRepositoryService.UpdateUserPasswordAsync(userId, A<string>._)).Returns(true);

            var result = await _userService.ResetPasswordAsync(userId, newPlainPassword);

            A.CallTo(() => _refreshTokenRepositoryService.RevokeAllRefreshTokens(userId)).MustHaveHappened();

            Assert.True(result);
        }
    }
}