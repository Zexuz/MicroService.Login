using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroService.Common.Core.Misc;
using MicroService.Login.Repo.MongoDb.Models;
using MicroService.Login.Security.Services.Impl;
using MicroService.Login.Security.Services.Interfaces;
using MicroService.Login.WebApi.Models;

namespace MicroService.Login.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class DomainVerificationController : Controller
    {
        private readonly IDomainVerificationService _domainVerificationService;

        public DomainVerificationController(IDomainVerificationService domainVerificationService)
        {
            _domainVerificationService = domainVerificationService;
        }

        /// <summary>
        ///  Create a request to be a verified domain user
        /// </summary>
        /// <remarks>
        /// This returns the code to be added to the users website meta-data tags.
        /// </remarks>
        /// <param name="model">The model</param>
        /// <response code="200">The request was proccessed and saved</response>
        /// <response code="400">The website is not a valid url</response>
        /// <response code="409">There is alredy a active request </response>
        /// <response code="409">The user has already a verified domain</response>
        [HttpPost("domain")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<DomainScraper>), 200)]
        public async Task<IActionResult> Init([FromBody] InitDomainVerificationModel model)
        {
            if(!Uri.TryCreate(model.Website,UriKind.Absolute,out var websiteUri))
                return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainInvalidWebsite});
            
            if(websiteUri.Scheme.ToLower() != "https")
                return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainNotHttps});
            
            var res = await _domainVerificationService.Init(int.Parse(User.Identity.Name), $"{websiteUri.Scheme}://{websiteUri.Host}");

            switch (res.ErrorType)
            {
                case VerifyDomainRequest.Error.PeningVerificationAlreadyExist:
                    return StatusCode(409, new ErrorResult<string> {Error = StrResource.DomainAlreadyPending});
                case VerifyDomainRequest.Error.AlreadyDomainVerified:
                    return StatusCode(409, new ErrorResult<string> {Error = StrResource.DomainUserAlreadyVerified});
                case null:
                    return Ok(new SuccessResult<DomainScraper> {Data = res.Data});
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///  Send the crawler to the users website
        /// </summary>
        /// <remarks>
        /// Gets the users pending request to be validated and crawls to the website, and if the code is correct, upgrades  the user to a domain verified user
        /// </remarks>
        /// <response code="200">The user is now verified</response>
        /// <response code="400">A error occured, so return error message for details</response>
        [HttpPost("crawl")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<string>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 400)]
        public async Task<IActionResult> Crawl()
        {
            var res = await _domainVerificationService.VerifyDomain(int.Parse(User.Identity.Name));
            return ValidationErrorResponse(res);
        }

        /// <summary>
        ///  Get the pending request 
        /// </summary>
        /// <remarks>
        /// Gets the users pending request if it exists
        /// </remarks>
        /// <response code="200">Returns the request details</response>
        /// <response code="404">No pending request exists</response>
        [HttpGet("domain")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<DomainScraper>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 404)]
        public async Task<IActionResult> PendingCrawl()
        {
            var res = await _domainVerificationService.GetPendingAsync(int.Parse(User.Identity.Name));
            if (res == null)
                return NotFound(new SuccessResult<string> {Data = StrResource.DomainNoPendingVerification});
            return Ok(new SuccessResult<DomainScraper> {Data = res});
        }

        /// <summary>
        ///  Delete the pending request 
        /// </summary>
        /// <remarks>
        /// Deletes the users pending request if it exists
        /// </remarks>
        /// <response code="200">Request deleated</response>
        /// <response code="404">No pending request exists</response>
        [HttpDelete("domain")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SuccessResult<DomainScraper>), 200)]
        [ProducesResponseType(typeof(ErrorResult<string>), 404)]
        public async Task<IActionResult> DeletePendingCrawl()
        {
            var res = await _domainVerificationService.DeletePendingAsync(int.Parse(User.Identity.Name));
            if (!res)
                return NotFound(new SuccessResult<string> {Data = StrResource.DomainNoPendingVerification});
            return Ok(new SuccessResult<string> {Data = StrResource.DomainRequestDeleted});
        }


        private IActionResult ValidationErrorResponse(VerifyDomainResult verifyDomainResult)
        {
            switch (verifyDomainResult.ErrorType)
            {
                case VerifyDomainResult.Error.WrongCode:
                    return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainWrongCode});
                case VerifyDomainResult.Error.NoCodeFound:
                    return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainNoCodeFound});
                case VerifyDomainResult.Error.InvalidWebsite:
                    return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainInvalidWebsite});
                case VerifyDomainResult.Error.NoPedningVerificationFound:
                    return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainNoPendingVerification});
                case VerifyDomainResult.Error.UnknowError:
                    return StatusCode(400, new ErrorResult<string> {Error = StrResource.DomainUnkownError});
                case null:
                    return StatusCode(200, new SuccessResult<string> {Data = StrResource.DomainVerified});
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}