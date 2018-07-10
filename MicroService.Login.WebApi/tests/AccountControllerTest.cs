using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Security;
using MicroService.Login.Security.Models;
using MicroService.Login.Security.Services.Interfaces;
using MicroService.Login.WebApi.Controllers.v1;
using MicroService.Login.WebApi.Models;
using Xunit;
using ConnectionInfo = MicroService.Login.Security.ConnectionInfo;

namespace MicroService.Login.Webapi.Test
{
    public class AccountControllerTest
    {
        private IUserService      _userService;
        private AccountController _accountController;

        public AccountControllerTest()
        {
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("testsetting.json");

            var config = builder.Build();

            _userService = A.Fake<IUserService>();
            var emailService = A.Fake<IEmailService>();
            var tokenValidationService = A.Fake<ITokenValidationService>();

            _accountController = new AccountController
            (
                _userService,
                config,
                emailService,
                tokenValidationService,
                A.Dummy<ITwoFactorAuthenticatorManager>(),
                A.Dummy<IJsonWebTokenService>()
            );

            ConnectionIp(_accountController);
        }

        private static void ConnectionIp(Controller controller, IPAddress address = null)
        {
            var ctx = new DefaultHttpContext();
            ctx.Connection.RemoteIpAddress = address ?? IPAddress.Loopback;
            ctx.Request.Headers.Add("User-Agent", "userAGentHeader");

            var actionContext = new ActionContext
            {
                HttpContext = ctx,
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            };

            controller.ControllerContext = new ControllerContext(actionContext);
        }


        [Fact]
        public async Task InvalidLoginDataReturns401()
        {
            var loginModel = new LoginModel
            {
                Password = "MyPassword",
                Email = "MyEmail@email.com",
            };

            A.CallTo(() => _userService.FindAsync(new Email(loginModel.Email))).Returns(new User());
            A.CallTo(() => _userService.LoginUser(A<Email>._, A<string>._, null, A<ConnectionInfo>._)).Returns(new LoginResponse
            {
                Error = LoginError.InvalidLoginDetails,
                OAuthToken = null,
                Success = false
            });

            var result = await _accountController.Login(loginModel);

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.IsAssignableFrom<ErrorResult<string>>(viewResult.Value);

            Assert.Equal(401, viewResult.StatusCode);
        }

        [Fact]
        public async Task ValidLoginDataReturnsTokenSuccess()
        {
            var loginModel = new LoginModel
            {
                Password = "ValidPassword",
                Email = "ValidEmil@emai.com"
            };

            A.CallTo(() => _userService.FindAsync(A<Email>.That.Matches(e => e.Value == loginModel.Email))).Returns(new User());
            A.CallTo(() => _userService.LoginUser(A<Email>._, A<string>._, null, A<ConnectionInfo>._)).Returns(new LoginResponse
            {
                Error = LoginError.None,
                OAuthToken = new OAuthToken
                {
                    AccessToken = "",
                    Expires = DateTimeOffset.Now,
                    RefreshToken = "",
                    Type = OAuthToken.OAuthTokenType.Bearer
                },
                Success = true
            });

            var result = await _accountController.Login(loginModel);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<SuccessResult<OAuthTokenResponse>>(viewResult.Value);
        }

        [Fact]
        public async Task UserHasNotValidatedEmailFails()
        {
            var loginModel = new LoginModel
            {
                Password = "ValidPassword",
                Email = "ValidEmil@emai.com"
            };

            A.CallTo(() => _userService.FindAsync(new Email(loginModel.Email))).Returns(new User());
            A.CallTo(() => _userService.LoginUser(A<Email>._, A<string>._, null, A<ConnectionInfo>._)).Returns(new LoginResponse
            {
                Error = LoginError.NotValidatedEmail,
                OAuthToken = null,
                Success = false
            });

            var result = await _accountController.Login(loginModel);

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.IsAssignableFrom<ErrorResult<string>>(viewResult.Value);
            Assert.Equal(403, viewResult.StatusCode);
        }

        [Fact]
        public async Task RegisterPasswordIsNotSameReturnsBadRequest()
        {
            var loginModel = new RegisterModel
            {
                Password = "match",
                PasswordRepeat = "noMatch"
            };

            var result = await _accountController.Register(loginModel);

            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<ErrorResult<string>>(viewResult.Value);
        }

        [Fact]
        public async Task CreateUserFailsReturnsConflict()
        {
            var registerModel = new RegisterModel
            {
                Username = "UserName",
                Password = "match",
                PasswordRepeat = "match",
                Email = "email@a.com"
            };

            A.CallTo(() => _userService.CreateUserAsync(A<User>._, A<IPAddress>._)).Returns(new CreateUserReponse(false));

            var result = await _accountController.Register(registerModel);

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.IsAssignableFrom<ErrorResult<string>>(viewResult.Value);
            Assert.Equal(409, viewResult.StatusCode);
        }

        [Fact]
        public async Task CreateUserSuccess()
        {
            var registerModel = new RegisterModel
            {
                Username = "UserName",
                Password = "match",
                PasswordRepeat = "match",
                Email = "email@a.com"
            };

            A.CallTo(() => _userService.CreateUserAsync(A<User>._, A<IPAddress>._)).Returns(new CreateUserReponse(true));

            var result = await _accountController.Register(registerModel);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<string>(viewResult.Value);
        }
    }
}