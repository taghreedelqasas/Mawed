using Maw3ed.BLL.DTOs.MedicalFiles;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicalFileController : ControllerBase
    {
        private readonly IMedicalFileService _medicalFileService;
        private readonly IUnitOfWork _unitOfWork;

        public MedicalFileController(
            IMedicalFileService medicalFileService,
            IUnitOfWork unitOfWork)
        {
            _medicalFileService = medicalFileService;
            _unitOfWork = unitOfWork;
        }

        // ============ Helpers ============
        private async Task<int?> GetPatientIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var patients = await _unitOfWork.GetRepository<Patient>()
                .FindAsync(p => p.UserId == userId);

            return patients.FirstOrDefault()?.Id;
        }

        private async Task<int?> GetDoctorIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var doctors = await _unitOfWork.GetRepository<Doctor>()
                .FindAsync(d => d.UserId == userId);

            return doctors.FirstOrDefault()?.Id;
        }

        // ============ Patient Endpoints ============

        // GET: api/MedicalFile
        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetAllFiles()
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            var files = await _medicalFileService.GetPatientFilesAsync(patientId.Value);
            return Ok(files);
        }

        // GET: api/MedicalFile/summary
        [HttpGet("summary")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetCategorySummary()
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            var summary = await _medicalFileService.GetCategorySummaryAsync(patientId.Value);
            return Ok(summary);
        }

        // GET: api/MedicalFile/category/LabResult
        [HttpGet("category/{category}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetFilesByCategory(MedicalFileCategory category)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            var files = await _medicalFileService
                .GetPatientFilesByCategoryAsync(patientId.Value, category);
            return Ok(files);
        }

        // GET: api/MedicalFile/5
        [HttpGet("{fileId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetFileById(int fileId)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            try
            {
                var file = await _medicalFileService.GetFileByIdAsync(fileId, patientId.Value);
                return Ok(file);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/MedicalFile
        [HttpPost]
        [Authorize(Roles = "Patient")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadMedicalFileDto dto)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            try
            {
                var result = await _medicalFileService.UploadFileAsync(patientId.Value, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/MedicalFile/5
        [HttpDelete("{fileId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            try
            {
                await _medicalFileService.DeleteFileAsync(fileId, patientId.Value);
                return Ok("تم مسح الملف");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ============ Doctor Endpoints ============

        // POST: api/MedicalFile/doctor-upload/5
        [HttpPost("doctor-upload/{patientId}")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DoctorUploadFile(
            int patientId, [FromForm] UploadMedicalFileDto dto)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            try
            {
                var result = await _medicalFileService
                    .DoctorUploadFileAsync(patientId, doctorId.Value, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/MedicalFile/doctor-view/5
        [HttpGet("doctor-view/{patientId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetFilesForDoctor(int patientId)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            try
            {
                var files = await _medicalFileService
                    .GetFilesForDoctorAsync(patientId, doctorId.Value);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
