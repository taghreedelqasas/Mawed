using Maw3ed.BLL.DTOs.Appointment;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET api/appointments/doctors/{doctorId}/available-slots  → 200
        [HttpGet("doctors/{doctorId}/available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int doctorId)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId);
            return Ok(slots);
        }

        // POST api/appointments  → 201 | 400 | 404 | 409
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _appointmentService.BookAppointmentAsync(CurrentUserId, dto);

            return result.ToCreatedResult(
                this,
                nameof(GetMyAppointments),
                new { },
                new { result.Message, result.Data });
        }

        // DELETE api/appointments/{id}/cancel  → 200 | 400 | 404 | 409
        [HttpDelete("{id}/cancel")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var result = await _appointmentService.CancelAppointmentAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        // PUT api/appointments/{id}/reschedule  → 200 | 400 | 404 | 409
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RescheduleAppointment(
            int id, [FromBody] RescheduleAppointmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _appointmentService.RescheduleAppointmentAsync(CurrentUserId, id, dto);
            return result.ToActionResult(this);
        }

        // GET api/appointments/my  → 200
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var result = await _appointmentService.GetPatientAppointmentsAsync(CurrentUserId);
            return Ok(result);
        }

        // GET api/appointments/doctor/my  → 200
        [HttpGet("doctor/my")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var result = await _appointmentService.GetDoctorAppointmentsAsync(CurrentUserId);
            return Ok(result);
        }

        // PATCH api/appointments/{id}/confirm  → 200 | 404 | 409
        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var result = await _appointmentService.ConfirmAppointmentAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        // PATCH api/appointments/{id}/complete  → 200 | 400 | 404
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var result = await _appointmentService.CompleteAppointmentAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        // PATCH api/appointments/{id}/no-show  → 200 | 400 | 404
        [HttpPatch("{id}/no-show")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            var result = await _appointmentService.MarkNoShowAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        // GET api/appointments/{id}  → 200 | 404
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var role = User.IsInRole("Doctor") ? "Doctor"
                     : User.IsInRole("Patient") ? "Patient"
                     : string.Empty;

            var result = await _appointmentService.GetAppointmentByIdAsync(CurrentUserId, role, id);
            return result.ToActionResult(this);
        }
    }
}
