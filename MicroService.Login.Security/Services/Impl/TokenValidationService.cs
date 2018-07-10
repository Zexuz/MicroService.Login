using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class TokenValidationService : ITokenValidationService
    {
        private readonly IJsonWebTokenService _jwtService;
        private readonly IHashService         _hashService;
        private readonly IUserService         _userService;

        public TokenValidationService(IJsonWebTokenService jwtService, IHashService hashService, IUserService userService)
        {
            _jwtService = jwtService;
            _hashService = hashService;
            _userService = userService;
        }

        public async Task<TokenValidationResult> IsIpWhitelistValid(string tokenString)
        {
            if (!TryDecodeToken(tokenString, out var token))
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidOrExpiered,
                    Success = false
                };

            if (token[CustomClaims.ValidationType] != "ipValidation")
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidValidationType,
                    Success = false
                };

            var userId = int.Parse(token[CustomClaims.Id]);
            var user = await _userService.FindAsync(userId);

            return new TokenValidationResult
            {
                Success = true,
                Token = token,
                TokenOwner = user
            };
        }


        public async Task<TokenValidationResult> IsEmailValidationValid(string tokenString)
        {
            if (!TryDecodeToken(tokenString, out var token))
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidOrExpiered,
                    Success = false
                };

            if (token[CustomClaims.ValidationType] != "emailValidation")
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidValidationType,
                    Success = false
                };

            var userId = int.Parse(token[CustomClaims.Id]);
            var user = await _userService.FindAsync(userId);


            return new TokenValidationResult
            {
                Success = true,
                Token = token,
                TokenOwner = user
            };
        }

        public async Task<TokenValidationResult> IsPasswordResetValid(string tokenString)
        {
            if (!TryDecodeToken(tokenString, out var token))
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidOrExpiered,
                    Success = false
                };

            if (token[CustomClaims.ValidationType] != "passwordReset")
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidValidationType,
                    Success = false
                };

            var userId = int.Parse(token[CustomClaims.Id]);
            var secretFromToken = token[CustomClaims.Secret];

            var user = await _userService.FindAsync(userId);

            var secret = _hashService.CreateBase64Sha512Hash(user.Password, user.Created.ToString());
            if (secretFromToken != secret)
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidSecret,
                    Success = false,
                };

            return new TokenValidationResult
            {
                TokenOwner = user,
                Success = true,
                Token = token
            };
        }

        public async Task<TokenValidationResult> IsAddTwoFactorAuthValid(string tokenString)
        {
            if (!TryDecodeToken(tokenString, out var token))
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidOrExpiered,
                    Success = false
                };

            if (token[CustomClaims.ValidationType] != "twoFactor")
                return new TokenValidationResult
                {
                    Error = TokenValidationError.InvalidValidationType,
                    Success = false
                };

            var userId = int.Parse(token[CustomClaims.Id]);
            var user = await _userService.FindAsync(userId);


            return new TokenValidationResult
            {
                Success = true,
                Token = token,
                TokenOwner = user
            };
        }

        private bool TryDecodeToken(string tokenString, out Dictionary<string, string> token)
        {
            try
            {
                token = _jwtService.DecodeToken<Dictionary<string, string>>(tokenString);
                return true;
            }
            catch (Exception)
            {
                token = null;
                return false;
            }
        }
    }
}