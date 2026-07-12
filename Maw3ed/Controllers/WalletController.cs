using Maw3ed.BLL.DTOs.Wallet;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.Api.Controllers
{
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IWithdrawService _withdrawService;

        public WalletController(IWalletService walletService, IWithdrawService withdrawService)
        {
            _walletService = walletService;
            _withdrawService = withdrawService;
            }

        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _walletService.GetWalletAsync(userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _walletService.GetTransactionsAsync(userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _withdrawService.CreateWithdrawRequestAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
