using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.APIs.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAvailabilityController : Controller
    {
        private readonly IDoctorAvailabilityManager _doctorAvailabilityManager;

        public DoctorAvailabilityController(IDoctorAvailabilityManager doctorAvailabilityManager)
        {
            _doctorAvailabilityManager = doctorAvailabilityManager;
        }

        // بس الدكتور يضيف مواعيده
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> Add(CreateDoctorAvailabilityDto dto)
        {
            try
            {
                await _doctorAvailabilityManager.AddAsync(dto);
                return Ok(new { Message = "Doctor availability added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> BulkAdd(BulkCreateDoctorAvailabilityDto dto)
        {
            try
            {
                await _doctorAvailabilityManager.BulkAddAsync(dto);
                return Ok(new { Message = "Doctor availability slots added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> Update(int id, UpdateDoctorAvailabilityDto dto)
        {
            try
            {
                dto.Id = id;
                await _doctorAvailabilityManager.UpdateAsync(dto);
                return Ok(new { Message = "Doctor availability updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _doctorAvailabilityManager.DeleteAsync(id);
                return Ok(new { Message = "Availability slot deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // أي حد يشوف المواعيد
        [HttpGet("doctor/{doctorId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByDoctor(int doctorId)
        {
            var slots = await _doctorAvailabilityManager.GetByDoctorAsync(doctorId);
            return Ok(slots);
        }

        [HttpGet("doctor/{doctorId}/available")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date)
        {
            var slots = await _doctorAvailabilityManager.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var slot = await _doctorAvailabilityManager.GetByIdAsync(id);
                return Ok(slot);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}