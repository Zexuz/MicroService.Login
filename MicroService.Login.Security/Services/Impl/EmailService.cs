using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MicroService.Common.Core.Models;
using MicroService.Common.Core.Services.impl;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSenderService  _emailSenderService;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public EmailService(IEmailSenderService emailSenderService, IJsonWebTokenService jsonWebTokenService)
        {
            _emailSenderService = emailSenderService;
            _jsonWebTokenService = jsonWebTokenService;
        }


        public async Task<bool> SendResetPasswordEmailAsync(User user, string baseUrl)
        {
            var token = _jsonWebTokenService.CreatePasswordResetToken(user);
            var emailRequest = CreateResetPasswordEmailRequest(user, token);
            return await SendEmailAsync(emailRequest);
        }

        public async Task<bool> SendWhitelistIpEmailAsync(IPAddress connectionIp, User user, string baseUrl)
        {
            var token = _jsonWebTokenService.CreateWhitelistIpValidationToken(user, connectionIp);
            var emailRequest = CreateValidationWhitelistIpRequest(user, token);
            return await SendEmailAsync(emailRequest);
        }

        public async Task<bool> SendValidationEmailAsync(User user, string baseUrl)
        {
            var token = _jsonWebTokenService.CreateEmailValidationToken(user);
            var emailRequest = CreateValidationEmailRequest(user, token);
            return await SendEmailAsync(emailRequest);
        }

        public async void SendResetPasswordEmail(User user, string baseUrl)
        {
            try
            {
                await SendResetPasswordEmailAsync(user, baseUrl);
            }
            catch (Exception e)
            {
                //TODO LOG ERROR
            }
        }

        public async void SendWhitelistIpEmail(IPAddress connectionIp, User user, string baseUrl)
        {
            try
            {
                await SendWhitelistIpEmailAsync(connectionIp, user, baseUrl);
            }
            catch (Exception e)
            {
                //TODO LOG ERROR
            }
        }

        public async void SendValidationEmail(User user, string baseUrl)
        {
            try
            {
                await SendValidationEmailAsync(user, baseUrl);
            }
            catch (Exception e)
            {
                //TODO LOG ERROR
            }
        }

        private async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                await _emailSenderService.SendEmailAsync(emailRequest);
                return true;
            }
            catch (Exception e)
            {
                //TODO log error
                return false;
            }
        }

        private static EmailRequest CreateResetPasswordEmailRequest(User detailedUser, Token token)
        {
            var subject = "Password reset";
            return CreateEmailRequest(token, detailedUser, subject);
        }

        private static EmailRequest CreateValidationWhitelistIpRequest(User detailedUser, Token token)
        {
            var subject = "Ip validation";

            return CreateEmailRequest(token, detailedUser, subject);
        }

        private static EmailRequest CreateValidationEmailRequest(User detailedUser, Token token)
        {
            var subject = "Register";
            return CreateEmailRequest(token, detailedUser, subject);
        }

        private static EmailRequest CreateEmailRequest(Token token, User user, string subject)
        {
            var sb = new StringBuilder();
            sb.Append($@"<p>Hello {user.Username}!</p><br/>");

            sb.Append($"<p>use this token in swagger to validate {token.TokenString}</p>");

            var body = sb.ToString();

            var emailRequest = new EmailRequest(user.Email, body, subject, true);
            return emailRequest;
        }
    }
}