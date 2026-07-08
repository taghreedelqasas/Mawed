// Maw3ed.Api/Controllers/AdminWithdrawController.cs
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/withdraw-requests")]
    public class AdminWithdrawController : ControllerBase
    {
        private readonly IAdminWithdrawService _service;

        public AdminWithdrawController(IAdminWithdrawService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingRequestsAsync();
            return Ok(result);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _service.ApproveAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _service.RejectAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}