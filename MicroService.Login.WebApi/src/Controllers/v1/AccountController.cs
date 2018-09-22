using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Misc;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Common.Core.Web;
using MicroService.Login.Models;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Security;
using MicroService.Login.Security.Models;
using MicroService.Login.Security.Services.Interfaces;
using MicroService.Login.WebApi.Models;

namespace MicroService.Login.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserService                   _userService;
        private readonly IEmailService                  _emailService;
        private readonly ITokenValidationService        _tokenValidationService;
        private readonly string                         _baseUrl;
        private readonly ITwoFactorAuthenticatorManager _twoFactorAuthenticatorManager;
        private readonly IJsonWebTokenService           _jsonWebTokenService;

        public AccountController
        (
            IUserService userService,
            IConfiguration configuration,
            IEmailService emailService,
            ITokenValidationService tokenValidationService,
            ITwoFactorAuthenticatorManager twoFactorAuthenticatorManager,
            IJsonWebTokenService jsonWebTokenService
        )
        {
            _userService = userService;
            _emailService = emailService;
            _tokenValidationService = tokenValidationService;
            _twoFactorAuthenticatorManager = twoFactorAuthenticatorManager;
            _jsonWebTokenService = jsonWebTokenService;

            var host = configuration["Hosting:Domain"];
            var scheme = configuration["Hosting:Scheme"];
            var port = int.Parse(configuration["Hosting:Port"]);

            _baseUrl = $"{scheme}://{host}:{port}";
        }

        /// <summary>
        ///  Whitelist a ip address
        /// </summary>
        /// <remarks>
        /// Use this to white list a users ip.
        /// </remarks>
        /// <param name="token">The JWT as a string</param>
        /// <response code="200">Whitelisted the ip</response>
        /// <response code="401">The JWT has expiered. A new password reset should be requested</response>
        [AllowAnonymous]
        [HttpPost("ip/{token}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ErrorResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        public async Task<IActionResult> WhitelistIp(string token)
        {
            var validationResult = await _tokenValidationService.IsIpWhitelistValid(token);

            if (!validationResult.Success) return ValidationErrorResponse(validationResult);

            var ipToWhitelist = IPAddress.Parse(validationResult.Token[CustomClaims.Ip]);
            await _userService.WhitelistIpAsync(validationResult.TokenOwner, ipToWhitelist);
            return Ok(new SuccessResult<string> {Data = StrResource.AddressWhitelistedSuccess});
        }

        /// <summary>
        ///  Remove a whitelist ip address
        /// </summary>
        /// <remarks>
        /// Use this to remove a white list a users ip.
        /// </remarks>
        /// <param name="ip">The ip as a string to remove from the whitelist</param>
        /// <response code="200">Whitelisted the ip</response>
        [HttpDelete("ip/{ip}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ErrorResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        [ProducesResponseType(typeof(ErrorResult<string>), 404)]
        public async Task<IActionResult> RemoveWhitelistedIp(string ip)
        {
            if (!IPAddress.TryParse(ip, out var ipAddress))
                return BadRequest(new ErrorResult<string> {Error = StrResource.InvalidIpFormat});

            var res = await _userService.RemoveWhitelistedIpAsync(int.Parse(User.Identity.Name), ipAddress);

            if (!res)
                return NotFound(new ErrorResult<string> {Error = StrResource.IpNotFound});

            return Ok(new SuccessResult<string> {Data = StrResource.WhitelistedIpRemoved});
        }

        /// <summary>
        ///  Remove the users refresh token
        /// </summary>
        /// <remarks>
        /// Invalidates the refreshtoken so that no new AccessToken can be generated with it.
        /// 
        /// This can also be used as a courtesy for when the user logout. Not really neccecary if the Refresh token is delete on the client side but helps the user to keep track of issed tokens.
        /// </remarks>
        /// <param name="refreshToken">The refresh token to remove</param>
        /// <response code="200">Refresh token is removed</response>
        [HttpDelete("refreshToken/{refreshToken}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> InvalidateRefreshToken(string refreshToken)
        {
            var userId = int.Parse(User.Identity.Name);
            await _userService.RevokeRefreshToken(userId, refreshToken);
            return Ok(new SuccessResult<string> {Data = StrResource.RevokeRefreshTokenSuccess});
        }


        /// <summary>
        ///  Loggs in the user
        /// </summary>
        /// <remarks>
        /// Use this to login a already registered user.
        /// </remarks>
        /// <param name="model">The login model</param>
        /// <response code="200">User is logged in</response>
        /// <response code="401">Invalid username,password or two factor code</response>
        /// <response code="403">Username and password was a match, but email is nor verified, If the user has not received a email. Use the resend endpoint.
        /// </response>
        [AllowAnonymous]
        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(OAuthToken), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        [ProducesResponseType(typeof(ErrorResult<string>), 403)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var connectionInfo = new ConnectionInfo
            {
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress,
                BrowserInfo = UserAgentParser.FromUserAgent(Request.Headers["User-Agent"])
            };
            var loginResponse = await _userService.LoginUser(new Email(model.Email), model.Password, model.TwoFactorCode, connectionInfo);

            if (!loginResponse.Success)
                switch (loginResponse.Error)
                {
                    case LoginError.InvalidLoginDetails:
                        return StatusCode(401, new ErrorResult<string> {Error = StrResource.UsernameOfPasswordInvalid});
                    case LoginError.NotValidatedEmail:
                        return StatusCode(403, new ErrorResult<string> {Error = StrResource.EmailNotVerified});
                    case LoginError.NotWhitelistedIp:
                        return StatusCode(403, new ErrorResult<string> {Error = StrResource.EmailWhitelistIpAddress});
                    case LoginError.TwoFactorRequiered:
                        return StatusCode(403, new ErrorResult<string> {Error = StrResource.TwoFactorCodeRequiered});
                    case LoginError.TwoFactorInvalid:
                        return StatusCode(401, new ErrorResult<string> {Error = StrResource.TwoFactorCodeInvalid});
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            var response = new OAuthTokenResponse
            {
                AccessToken = loginResponse.OAuthToken.AccessToken,
                Expires = loginResponse.OAuthToken.Expires.ToUnixTimeSeconds(),
                Type = loginResponse.OAuthToken.Type.ToString(),
                RefreshToken = loginResponse.OAuthToken.RefreshToken,
            };

            return Ok(new SuccessResult<OAuthTokenResponse> {Data = response});
        }

        /// <summary>
        ///  Password reset
        /// </summary>
        /// <remarks>
        /// Lets the user reset it's password with a issues JWT
        /// </remarks>
        /// <param name="model">The ResetPassword model</param>
        /// <param name="token">The JWT as a string</param>
        /// <response code="200">Users password was changed</response>
        /// <response code="400">Password does not match</response>
        /// <response code="401">The JWT has expiered. A new password reset should be requested</response>
        [AllowAnonymous]
        [HttpPost("password/reset/{token}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        [ProducesResponseType(typeof(ErrorResult<string>), 403)]
        public async Task<IActionResult> PasswordReset([FromBody] ResetPasswordModel model, string token)
        {
            if (model.Password != model.PasswordRepeat)
                return BadRequest(new ErrorResult<string> {Error = StrResource.PasswordDoesNotMatch});

            var validationResult = await _tokenValidationService.IsPasswordResetValid(token);

            if (!validationResult.Success) return ValidationErrorResponse(validationResult);

            if (await _userService.ResetPasswordAsync(validationResult.TokenOwner, model.Password))
                return Ok(new SuccessResult<string> {Data = StrResource.PasswordResetSuccess});

            var ex = new Exception("Can't update email status");
            ex.Data.Add("validationResult", validationResult);
            throw ex;
        }

        /// <summary>
        ///  Password reset JWT request
        /// </summary>
        /// <remarks>
        /// Sends a email with a JWT for password resets
        /// </remarks>
        /// <param name="model">The ResetPassword model</param>
        /// <response code="200">This is the only response, even if the email does not exist</response>
        [AllowAnonymous]
        [HttpPost("password/reset")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        [ProducesResponseType(typeof(ErrorResult<string>), 403)]
        public async Task<IActionResult> PasswordReset([FromBody] EmailModel model)
        {
            var user = await _userService.FindAsync(new Email(model.Email));

            if (user != null)
                _emailService.SendResetPasswordEmail(user, _baseUrl);

            return Ok(new SuccessResult<string> {Data = StrResource.EmailActivationSent});
        }

        /// <summary>
        ///  Validate a email with a JWT
        /// </summary>
        /// <remarks>
        /// Validates a users email with a JWT receeived in the users email.
        /// </remarks>
        /// <param name="token">The JWT as a string</param>
        /// <response code="200">The email is now verified and the user can login as usual</response>
        /// <response code="401">The token is invalid/to old or is not a emial validation token</response>
        [AllowAnonymous]
        [HttpPost("email/validation/{token}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 401)]
        public async Task<IActionResult> EmailActivation(string token)
        {
            var validationResult = await _tokenValidationService.IsEmailValidationValid(token);

            if (!validationResult.Success) return ValidationErrorResponse(validationResult);

            await _userService.UpdateEmailValidationStatusAsync(validationResult.TokenOwner.Id);
            return Ok(new SuccessResult<string> {Data = StrResource.EmailActivationSuccess});
        }


        /// <summary>
        ///  Sends a 'validate email' email.
        /// </summary>
        /// <remarks>
        /// Send a email verification JWT to the email.
        /// </remarks>
        /// <param name="model">The Email model</param>
        /// <response code="200">This is the only reponse</response>
        [AllowAnonymous]
        [HttpPost("email/validation/resend")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> ResendEmailActivation([FromBody] EmailModel model)
        {
            var user = await _userService.FindAsync(new Email(model.Email));
            if (user == null)
                return Ok(new SuccessResult<string> {Data = StrResource.EmailActivationSent});

            if (!await _userService.HasVerfifiedEmailAsync(user))
                _emailService.SendValidationEmail(user, _baseUrl);

            return Ok(new SuccessResult<string> {Data = StrResource.EmailActivationSent});
        }

        /// <summary>
        ///  Registers a user
        /// </summary>
        /// <remarks>
        /// Registers the user to our system
        /// </remarks>
        /// <param name="model">The Register model</param>
        /// <response code="200">This is the only reponse</response>
        /// <response code="400">The password does not match</response>
        /// <response code="409">Creating the user failed, most like due to a confilict</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 400)]
        [ProducesResponseType(typeof(ErrorResult<string>), 409)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model.Password != model.PasswordRepeat)
                return BadRequest(new ErrorResult<string> {Error = StrResource.PasswordDoesNotMatch});

            var user = new User
            {
                Email = new Email(model.Email),
                Password = model.Password,
                Username = model.Username,
                Created = DateTimeOffset.Now,
                Role = Role.Admin
            };
            var connectionIp = Request.HttpContext.Connection.RemoteIpAddress;

            var createUserReponse = await _userService.CreateUserAsync(user, connectionIp);
            if (!createUserReponse.Success)
                return StatusCode(409, new ErrorResult<string> {Error = createUserReponse.Error});

            return Ok(StrResource.EmailActivationSent);
        }

        private IActionResult ValidationErrorResponse(TokenValidationResult validationResult)
        {
            switch (validationResult.Error)
            {
                case TokenValidationError.InvalidOrExpiered:
                    return StatusCode(401, new ErrorResult<string> {Error = StrResource.TokenInvalidOrToOld});
                case TokenValidationError.InvalidSecret:
                    return StatusCode(401, new ErrorResult<string> {Error = StrResource.TokenInvalidOrToOld});
                case TokenValidationError.InvalidValidationType:
                    return StatusCode(401, new ErrorResult<string> {Error = StrResource.TokenInvalidOrToOld});
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}