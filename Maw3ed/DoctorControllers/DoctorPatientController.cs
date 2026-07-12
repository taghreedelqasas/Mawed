using Maw3ed.BLL;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.APIs.DoctorControllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorPatientController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorPatientController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET /api/Doctor
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var filter = new DoctorSearchFilterDto();
            var doctors = await _doctorService.SearchDoctorsAsync(filter);
            return Ok(doctors);
        }

     

        // GET /api/Doctor/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetDoctorProfileAsync(id);
            if (doctor == null)
                return NotFound("الدكتور مش موجود");
            return Ok(doctor);
        }
        // GET /api/Doctor/search?specialty=&location=&doctorName=&sortBy=
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DoctorSearchFilterDto filter)
        {
            var doctors = await _doctorService.SearchDoctorsAsync(filter);
            return Ok(doctors);
        }
    }
}
