using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Common.Core.Web;
using MicroService.Login.Repo.MongoDb;
using MicroService.Login.Repo.MongoDb.Models;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class DomainVerificationService : IDomainVerificationService
    {
        private readonly IHttpRequestParser                   _httpRequestParser;
        private readonly IDomainScraperRepositoryService      _domainScraperRepositoryService;
        private readonly IVerifiedDomainUserRepositoryService _domainUserRepoService;
        private readonly IUserRepositoryService               _userRepositoryService;
        private readonly ITransactionFactory                  _transactionFactory;

        public DomainVerificationService
        (
            IHttpRequestParser httpRequestParser,
            IDomainScraperRepositoryService domainScraperRepositoryService,
            IVerifiedDomainUserRepositoryService domainUserRepoService,
            IUserRepositoryService userRepositoryService,
            ITransactionFactory transactionFactory
        )
        {
            _httpRequestParser = httpRequestParser;
            _domainScraperRepositoryService = domainScraperRepositoryService;
            _domainUserRepoService = domainUserRepoService;
            _userRepositoryService = userRepositoryService;
            _transactionFactory = transactionFactory;
        }

        public async Task<VerifyDomainRequest> Init(int userId, string website)
        {
            var user = await _userRepositoryService.FindAsync(userId);
            if (user.DomainId != null)
                return new VerifyDomainRequest
                {
                    Data = null,
                    ErrorType = VerifyDomainRequest.Error.AlreadyDomainVerified,
                    Success = false
                };

            if (await GetPendingAsync(userId) != null)
                return new VerifyDomainRequest
                {
                    Data = null,
                    ErrorType = VerifyDomainRequest.Error.PeningVerificationAlreadyExist,
                    Success = false
                };

            var obj = new DomainScraper
            {
                Code = Guid.NewGuid(),
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Website = website
            };
            await _domainScraperRepositoryService.InsertAsync(obj);

            return new VerifyDomainRequest
            {
                Data = obj,
                Success = true
            };
        }

        public async Task<VerifyDomainResult> VerifyDomain(int userId)
        {
            var result = await _domainScraperRepositoryService.FindByUserId(userId);

            if (result == null)
                return new VerifyDomainResult
                {
                    ErrorType = VerifyDomainResult.Error.NoPedningVerificationFound,
                    Success = false
                };

            var scrapeResult = await GetAndVerifyCode(result);
            if (!scrapeResult.Success) return scrapeResult;

            using (var transacrionWrapper = _transactionFactory.BeginTransaction())
            {
                var success = await InsertWithTransaction(userId, result, transacrionWrapper);
                if (!success)
                {
                    transacrionWrapper.Rollback();
                    return new VerifyDomainResult
                    {
                        ErrorType = VerifyDomainResult.Error.UnknowError,
                        Success = false
                    };
                }

                transacrionWrapper.Commit();
            }

            await _domainScraperRepositoryService.DeleteAsync(result.Id);

            return scrapeResult;
        }

        public async Task<bool> DeletePendingAsync(int userId)
        {
            var domain = await _domainScraperRepositoryService.FindByUserId(userId);
            if (domain == null)
                return false;
            await _domainScraperRepositoryService.DeleteAsync(domain.Id);
            return true;
        }

        public async Task<DomainScraper> GetPendingAsync(int userId)
        {
            return await _domainScraperRepositoryService.FindByUserId(userId);
        }

        private async Task<bool> InsertWithTransaction(int userId, DomainScraper result, ITransactionWrapper transacrionWrapper)
        {
            try
            {
                var insertResponse = await _domainUserRepoService.InsertAsync(result.UserId, result.Website, transacrionWrapper.Transaction);
                await _userRepositoryService.AddDomainAsync(userId, insertResponse.Id, transacrionWrapper.Transaction);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private async Task<VerifyDomainResult> GetAndVerifyCode(DomainScraper website)
        {
            var html = await GetWebsiteHtml(website.Website);

            if (html == null)
                return new VerifyDomainResult
                {
                    ErrorType = VerifyDomainResult.Error.InvalidWebsite,
                    Success = false
                };

            if (!TryFindCode(html, out var code))
                return new VerifyDomainResult
                {
                    ErrorType = VerifyDomainResult.Error.NoCodeFound,
                    Success = false
                };

            if (code != website.Code.ToString())
                return new VerifyDomainResult
                {
                    ErrorType = VerifyDomainResult.Error.WrongCode,
                    Success = false,
                };

            return new VerifyDomainResult
            {
                Success = true
            };
        }


        private bool TryFindCode(string html, out string code)
        {
            code = "";
            var regEx = new Regex(
                "<meta name=\"microServiceAuth\" content=\"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\"");
            var matches = regEx.Matches(html);
            if (matches.Count != 1) return false;

            if (matches[0].Groups.Count != 2) return false;

            code = matches[0].Groups[1].Value;
            return true;
        }

        private async Task<string> GetWebsiteHtml(string website)
        {
            var requestMessage = new RequestMessage(HttpMethod.Get, website);
            try
            {
                var html = await _httpRequestParser.ExecuteRawAsync(requestMessage);
                return html;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class VerifyDomainRequest
    {
        public bool          Success   { get; set; }
        public Error?        ErrorType { get; set; }
        public DomainScraper Data      { get; set; }

        public enum Error
        {
            PeningVerificationAlreadyExist,
            AlreadyDomainVerified,
        }
    }

    public class VerifyDomainResult
    {
        public bool   Success   { get; set; }
        public Error? ErrorType { get; set; }

        public enum Error
        {
            WrongCode,
            NoCodeFound,
            InvalidWebsite,
            NoPedningVerificationFound,
            UnknowError
        }
    }
}