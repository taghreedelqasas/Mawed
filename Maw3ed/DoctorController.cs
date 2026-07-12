using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.APIs.DoctorController
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorManager _doctorManager;

        public DoctorController(IDoctorManager doctorManager)
        {
            _doctorManager = doctorManager;
        }

        // أي حد يشوف الدكاترة
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorManager.GetAllAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorManager.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();
            return Ok(doctor);
        }
     
        // بس الأدمن يضيف/يعدل/يمسح
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DoctorCreateDto doctorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _doctorManager.AddAsync(doctorDto);
            return Ok(new { message = "Doctor created successfully!" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, DoctorUpdateDto doctorDto)
        {
            if (id != doctorDto.Id)
                return BadRequest("Id mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _doctorManager.UpdateAsync(doctorDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorManager.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var doctor = await _doctorManager.GetByUserIdAsync(userId);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPut("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateMyProfile(DoctorUpdateDto doctorDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _doctorManager.UpdateOwnProfileAsync(userId, doctorDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}