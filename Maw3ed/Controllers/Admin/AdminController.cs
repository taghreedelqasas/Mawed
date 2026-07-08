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

        public AdminController(IDoctorManager doctorManager, IAdminDashboardService dashboardService)
        {
            _doctorManager = doctorManager;
            _dashboardService = dashboardService;
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
    }
}