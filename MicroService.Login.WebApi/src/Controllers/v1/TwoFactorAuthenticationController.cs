using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Misc;
using MicroService.Login.Security;
using MicroService.Login.Security.Services.Interfaces;
using MicroService.Login.WebApi.Models;

namespace MicroService.Login.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class TwoFactorAuthenticationController : Controller
    {
        private readonly IUserService                   _userService;
        private readonly ITokenValidationService        _tokenValidationService;
        private readonly ITwoFactorAuthenticatorManager _twoFactorAuthenticatorManager;
        private readonly IJsonWebTokenService           _jsonWebTokenService;

        public TwoFactorAuthenticationController
        (
            IUserService userService,
            ITokenValidationService tokenValidationService,
            ITwoFactorAuthenticatorManager twoFactorAuthenticatorManager,
            IJsonWebTokenService jsonWebTokenService
        )
        {
            _userService = userService;
            _tokenValidationService = tokenValidationService;
            _twoFactorAuthenticatorManager = twoFactorAuthenticatorManager;
            _jsonWebTokenService = jsonWebTokenService;
        }

        [HttpGet("test/{code}")]
        [Produces("application/json")]
        public async Task<IActionResult> Test2Fa(string code)
        {
            var user = await _userService.FindAsync(int.Parse(User.Identity.Name));

            var isValid = _twoFactorAuthenticatorManager.VerifyCode(code, user.TwoFactorSecret, user.Email.Value);

            return Ok(new {IsValid = isValid});
        }


        /// <summary>
        ///  Enables 2fa for the current user
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="model">The model</param>
        /// <response code="200">2fa is now activated for the user</response>
        /// <response code="400">The code provided is invalid</response>
        /// <response code="400">The token provided is invalid</response>
        [HttpPost("add")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CreateTwoFactorResponseModel), 200)]
        public async Task<IActionResult> Add2Fa([FromBody] AddTwoFactorModel model)
        {
            var validationResult = await _tokenValidationService.IsAddTwoFactorAuthValid(model.Token);
            if (!validationResult.Success)
                return BadRequest("Invalid token");
            var user = validationResult.TokenOwner;
            var secret = validationResult.Token[CustomClaims.Secret];

            var isValid = _twoFactorAuthenticatorManager.VerifyCode(model.Code, secret, user.Email.Value);

            if (!isValid) return BadRequest();

            await _userService.EnableTwoFactorForUser(user, secret);

            return Ok();
        }

        /// <summary>
        ///  Removes 2fa for the current user
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="code">The code provided from the users 2fa device</param>
        /// <response code="200">2fa is now removed for the user</response>
        /// <response code="400">The user has no enabled 2fa</response>
        /// <response code="401">The code provieded bu the user was invaliad</response>
        [HttpDelete("remove/{code}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Remove2Fa(string code)
        {
            var user = await _userService.FindAsync(int.Parse(User.Identity.Name));

            if (string.IsNullOrEmpty(user.TwoFactorSecret))
                return BadRequest(new ErrorResult<string> {Error = StrResource.NoTwoWayAuthIsConfigurated});

            if (_twoFactorAuthenticatorManager.VerifyCode(code, user.TwoFactorSecret, user.Email.Value))
                return Unauthorized();

            if (await _userService.DisableTwoFactorForUser(user))
                return Ok();

            var ex = new Exception("Can't remove 2fa from user");
            ex.Data.Add(nameof(user), user);
            throw ex;
        }

        /// <summary>
        ///  Generates a secret for the user
        /// </summary>
        /// <remarks>
        ///  Generates a secret which the users scanns with the device.
        /// </remarks>
        /// <response code="200">Secret and token generated</response>
        [HttpGet("Initialization")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CreateTwoFactorResponseModel), 200)]
        public async Task<IActionResult> Get2FaInfo()
        {
            var user = await _userService.FindAsync(int.Parse(User.Identity.Name));

            if (!_twoFactorAuthenticatorManager.GenerateResult(user.Email.Value, "localhost", out var result))
                throw new Exception("Can't generate valid 2FA data");

            var token = _jsonWebTokenService.CreateTwoFactorToken(user, result.Secret);

            return Ok(new SuccessResult<CreateTwoFactorResponseModel>
            {
                Data = new CreateTwoFactorResponseModel
                {
                    Base64QrCode = QrCodeGenerator.StringAsBase64ImageData(result.ToString()),
                    Secret = result.Secret,
                    SecurityToken = token.TokenString
                }
            });
        }
    }
}