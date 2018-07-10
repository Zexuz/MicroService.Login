using System;
using System.Collections.Generic;
using System.Net;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.Services.impl;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Security.Helpers;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class JsonWebTokenService : JsonWebTokenBase, IJsonWebTokenService
    {
        private readonly TimeSpan     _defaultLifetime;
        private readonly IHashService _hashService;

        public JsonWebTokenService(string secret, TimeSpan defaultLifetime, IHashService hashService) : base(secret)
        {
            _defaultLifetime = defaultLifetime;
            _hashService = hashService;
        }

        public Token CreateToken(Dictionary<string, object> claims, TimeSpan lifespan)
        {
            var exp = DateTimeOffset.UtcNow.Add(lifespan);

            claims.Add("exp", exp.ToUnixTimeSeconds());
            var tokenString = CreateToken(claims);

            return new Token
            {
                Audience = null,
                Issuer = null,
                Claims = claims,
                Expires = exp,
                Lifespan = lifespan,
                TokenString = tokenString
            };
        }

        public Token CreateLoginToken(User user, IPAddress ipAddress, string audience, string issuer, TimeSpan? lifespan = null)
        {
            if (!lifespan.HasValue)
                lifespan = _defaultLifetime;

            var exp = DateTimeOffset.UtcNow.Add(lifespan.Value);

            var claims = new Dictionary<string, object>
            {
                {CustomClaims.Role, RoleParser.ToInt(user.Role)},
                {CustomClaims.Ip, ipAddress.ToString()},
                {CustomClaims.Id, user.Id},
                {"exp", exp.ToUnixTimeSeconds()}
            };

            if (!string.IsNullOrEmpty(audience))
                claims.Add("aud", audience);

            if (!string.IsNullOrEmpty(issuer))
                claims.Add("iss", issuer);

            var tokenString = CreateToken(claims);

            return new Token
            {
                Audience = audience,
                Claims = claims,
                Expires = exp,
                Issuer = issuer,
                Lifespan = lifespan,
                TokenString = tokenString
            };
        }

        public Token CreatePasswordResetToken(User user)
        {
            var secret = _hashService.CreateBase64Sha512Hash(user.Password, user.Created.ToString());

            var claims = new Dictionary<string, object>
            {
                {CustomClaims.ValidationType, "passwordReset"},
                {CustomClaims.Secret, secret},
                {CustomClaims.Id, user.Id},
            };
            return CreateValidationToken(claims);
        }

        public Token CreateWhitelistIpValidationToken(User user, IPAddress address)
        {
            var claims = new Dictionary<string, object>
            {
                {CustomClaims.ValidationType, "ipValidation"},
                {CustomClaims.Ip, address.ToString()},
                {CustomClaims.Id, user.Id},
            };
            return CreateValidationToken(claims);
        }

        public Token CreateEmailValidationToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                {CustomClaims.ValidationType, "emailValidation"},
                {CustomClaims.Id, user.Id},
            };
            return CreateValidationToken(claims);
        }

        public Token CreateTwoFactorToken(User user, string secret)
        {
            var claims = new Dictionary<string, object>
            {
                {CustomClaims.ValidationType, "twoFactor"},
                {CustomClaims.Id, user.Id},
                {CustomClaims.Secret, secret},
            };
            return CreateValidationToken(claims);
        }

        private Token CreateValidationToken(Dictionary<string, object> claims)
        {
            return CreateToken(claims, TimeSpan.FromMinutes(10));
        }
    }
}