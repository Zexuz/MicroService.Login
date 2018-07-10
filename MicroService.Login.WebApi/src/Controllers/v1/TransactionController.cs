using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService       _transactionService;
        private readonly ITransactionLookupService _transactionLookupService;

        public TransactionController(ITransactionService transactionService, ITransactionLookupService transactionLookupService)
        {
            _transactionService = transactionService;
            _transactionLookupService = transactionLookupService;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendCoins([FromBody] SendCoinsModel model)
        {
            await _transactionService.SendCoinsAsync(int.Parse(User.Identity.Name), model.ToUserId, model.NrOfCoins);
            return Ok();
        }

        [HttpGet("history/sent/{page}")]
        public async Task<IActionResult> TransactionHistorySent(int page)
        {
            var userId = int.Parse(User.Identity.Name);
            var res = await _transactionLookupService.GetSentAsync(userId, page);
            return Ok(res);
        }

        [HttpGet("history/received/{page}")]
        public async Task<IActionResult> TransactionHistoryReceived(int page)
        {
            var userId = int.Parse(User.Identity.Name);
            var res = await _transactionLookupService.GetReceivedAsync(userId, page);
            return Ok(res);
        }

        [HttpGet("history/{page}")]
        public async Task<IActionResult> TransactionHistory(int page)
        {
            var userId = int.Parse(User.Identity.Name);
            var res = await _transactionLookupService.GetAsync(userId, page);
            return Ok(res);
        }
    }

    public class SendCoinsModel
    {
        [Required]
        public int ToUserId { get; set; }

        [Required]
        public int NrOfCoins { get; set; }
    }
}