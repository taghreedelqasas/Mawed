using Microsoft.AspNetCore.Mvc;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System;
using System.Threading.Tasks; // إضافة المكتبة

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

        // POST: api/DoctorAvailability
        [HttpPost]
        public async Task<ActionResult> Add(CreateDoctorAvailabilityDto dto) // تعديل هنا
        {
            try
            {
                await _doctorAvailabilityManager.AddAsync(dto); // تعديل هنا
                return Ok(new { Message = "Doctor availability added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: api/DoctorAvailability/doctor/5
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult> GetByDoctor(int doctorId) // تعديل هنا
        {
            var slots = await _doctorAvailabilityManager.GetByDoctorAsync(doctorId); // تعديل هنا
            return Ok(slots);
        }

        // GET: api/DoctorAvailability/doctor/5/available
        [HttpGet("doctor/{doctorId}/available")]
        public async Task<ActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date) // تعديل هنا
        {
            var slots = await _doctorAvailabilityManager.GetAvailableSlotsAsync(doctorId, date); // تعديل هنا
            return Ok(slots);
        }

        // GET: api/DoctorAvailability/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id) // تعديل هنا
        {
            try
            {
                var slot = await _doctorAvailabilityManager.GetByIdAsync(id); // تعديل هنا
                return Ok(slot);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // DELETE: api/DoctorAvailability/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id) // تعديل هنا
        {
            try
            {
                await _doctorAvailabilityManager.DeleteAsync(id); // تعديل هنا
                return Ok(new { Message = "Availability slot deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: api/DoctorAvailability/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateDoctorAvailabilityDto dto) // تعديل هنا
        {
            try
            {
                dto.Id = id;
                await _doctorAvailabilityManager.UpdateAsync(dto); // تعديل هنا
                return Ok(new { Message = "Doctor availability updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/DoctorAvailability/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult> BulkAdd(BulkCreateDoctorAvailabilityDto dto) // تعديل هنا
        {
            try
            {
                await _doctorAvailabilityManager.BulkAddAsync(dto); // تعديل هنا
                return Ok(new { Message = "Doctor availability slots added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}