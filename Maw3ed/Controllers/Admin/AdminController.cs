using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Maw3ed.BLL.Services.Interfaces;
namespace Maw3ed.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IDoctorManager _doctorManager;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IAdminAppointmentService _appointmentService;

        public AdminController(IDoctorManager doctorManager, IAdminDashboardService dashboardService, IAdminAppointmentService appointmentService)
        {
            _doctorManager = doctorManager;
            _dashboardService = dashboardService;
            _appointmentService = appointmentService;
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
        // PUT: api/admin/reject-doctor/{userId}
        // Admin rejects a pending doctor request. body: { "reason": "..." } (optional)
        [HttpPut("reject-doctor/{userId}")]
        public async Task<IActionResult> RejectDoctor(string userId, [FromBody] RejectDoctorDto? dto)
        {
            var (success, message) = await _doctorManager.RejectDoctorAsync(userId, dto?.Reason);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
        // GET: api/admin/dashboard/overview
        // كروت الـ KPI اللي فوق في شاشة "نظرة عامة" (مواعيد اليوم - إجمالي المرضى - الإيرادات ...إلخ)
        [HttpGet("dashboard/overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var overview = await _dashboardService.GetOverviewAsync();
            return Ok(overview);
        }

        // GET: api/admin/dashboard/appointments-distribution?year=2026&month=7
        // بيانات الدونات "توزيع حالات المواعيد". لو مبعتش year/month بياخد الشهر الحالي.
        [HttpGet("dashboard/appointments-distribution")]
        public async Task<IActionResult> GetAppointmentsDistribution([FromQuery] int? year, [FromQuery] int? month)
        {
            var distribution = await _dashboardService.GetAppointmentStatusDistributionAsync(year, month);
            return Ok(distribution);
        }

        // GET: api/admin/dashboard/monthly-trend?type=appointments&months=12
        // بيانات خط "اتجاه المواعيد الشهري" (type = appointments أو revenue), افتراضيًا آخر 12 شهر.
        [HttpGet("dashboard/monthly-trend")]
        public async Task<IActionResult> GetMonthlyTrend([FromQuery] string type = "appointments", [FromQuery] int months = 12)
        {
            var trend = await _dashboardService.GetMonthlyTrendAsync(type, months);
            return Ok(trend);
        }

        [HttpGet("patients")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _dashboardService.GetAllPatientsAsync();
            return Ok(patients);
        }

        [HttpGet("doctors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _dashboardService.GetAllDoctorsAsync();
            return Ok(doctors);
        }
        // ============ جديد: شاشة "إدارة المواعيد" ============

        // GET: api/admin/appointments?page=1&pageSize=6&status=Completed&date=2026-07-12&search=احمد
        // كل الـ query params اختيارية. status لازم تكون قيمة من AppointmentStatus
        // (Pending / Confirmed / Completed / Cancelled)، لو اتبعتت قيمة غلط بيتجاهلها.

[HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 6,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string? search = null)
        {
            var result = await _appointmentService.GetAppointmentsAsync(page, pageSize, status, date, search);
            return Ok(result);
        }

        // GET: api/admin/appointments/summary
        // بيانات الكروت الـ 4 فوق الجدول (حجوزات اليوم + عدد كل حالة).
        [HttpGet("appointments/summary")]
        [HttpGet("appointments-summary")]
        public async Task<IActionResult> GetAppointmentsSummary()
        {
            var summary = await _appointmentService.GetSummaryAsync();
            return Ok(summary);
        }

        // ============ جديد: "عرض الملف الشخصي" ============

        // GET: api/admin/patients/{id}
        [HttpGet("patients/{id:int}")]
        public async Task<IActionResult> GetPatientDetail(int id)
        {
            var patient = await _dashboardService.GetPatientDetailAsync(id);
            if (patient == null) return NotFound(new { message = "Patient not found." });
            return Ok(patient);
        }

        // GET: api/admin/doctors/{id}
        [HttpGet("doctors/{id:int}")]
        public async Task<IActionResult> GetDoctorDetail(int id)
        {
            var doctor = await _dashboardService.GetDoctorDetailAsync(id);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });
            return Ok(doctor);
        }
    }
}