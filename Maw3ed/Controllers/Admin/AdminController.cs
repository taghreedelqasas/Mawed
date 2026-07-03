using Maw3ed.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IDoctorManager _doctorManager;

        public AdminController(IDoctorManager doctorManager)
        {
            _doctorManager = doctorManager;
        }

        // GET: api/admin/pending-doctors
        // Returns all doctors waiting for approval.
        [HttpGet("pending-doctors")]
        public async Task<IActionResult> GetPendingDoctors()
        {
            var doctors = await _doctorManager.GetPendingDoctorsAsync();
            return Ok(doctors);
        }

        // PUT: api/admin/approve-doctor/{userId}
        // Admin approves a doctor so they can log in.
        [HttpPut("approve-doctor/{userId}")]
        public async Task<IActionResult> ApproveDoctor(string userId)
        {
            var (success, message) = await _doctorManager.ApproveDoctorAsync(userId);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
