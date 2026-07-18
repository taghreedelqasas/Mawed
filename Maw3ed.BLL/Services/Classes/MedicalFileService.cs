using Maw3ed.BLL.DTOs.MedicalFiles;
using Maw3ed.BLL.Helpers;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{
    public class MedicalFileService : IMedicalFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _uploadsPath;

        public MedicalFileService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;

            var basePath = environment.WebRootPath
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            _uploadsPath = Path.Combine(basePath, "uploads");
        }

        // ============ Helper ============

        // بتتحقق إن فيه موعد Confirmed أو Completed بين الدكتور والمريض
        private async Task EnsureDoctorHasAppointmentWithPatientAsync(int patientId, int doctorId)
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .FindAsync(a => a.PatientId == patientId
                             && a.DoctorId == doctorId
                             && (a.Status == AppointmentStatus.Confirmed
                                 || a.Status == AppointmentStatus.Completed));

            if (!appointments.Any())
                throw new Exception("مينفعش تتعامل مع ملفات المريض ده، مفيش موعد بينكم");
        }

        // ============ Patient Endpoints ============

        public async Task<IEnumerable<MedicalFileDto>> GetPatientFilesAsync(int patientId)
        {
            var files = await _unitOfWork.GetRepository<MedicalFile>()
                .FindAsync(f => f.PatientId == patientId);

            return files.Select(f => MapToDto(f));
        }

        public async Task<IEnumerable<MedicalFileDto>> GetPatientFilesByCategoryAsync(
            int patientId, MedicalFileCategory category)
        {
            var files = await _unitOfWork.GetRepository<MedicalFile>()
                .FindAsync(f => f.PatientId == patientId && f.Category == category);

            return files.Select(f => MapToDto(f));
        }

        public async Task<MedicalFileDto> GetFileByIdAsync(int fileId, int patientId)
        {
            var file = await _unitOfWork.GetRepository<MedicalFile>()
                .GetByIdAsync(fileId);

            if (file == null || file.PatientId != patientId)
                throw new Exception("الملف مش موجود");

            return MapToDto(file);
        }

        public async Task<FileCategorySummaryDto> GetCategorySummaryAsync(int patientId)
        {
            var files = await _unitOfWork.GetRepository<MedicalFile>()
                .FindAsync(f => f.PatientId == patientId);

            return new FileCategorySummaryDto
            {
                LabResultCount = files.Count(f => f.Category == MedicalFileCategory.LabResult),
                ScanCount = files.Count(f => f.Category == MedicalFileCategory.Scan),
                PrescriptionCount = files.Count(f => f.Category == MedicalFileCategory.Prescription),
                MedicalReportCount = files.Count(f => f.Category == MedicalFileCategory.MedicalReport),
                TotalCount = files.Count()
            };
        }

        public async Task<MedicalFileDto> UploadFileAsync(int patientId, UploadMedicalFileDto dto)
        {
            if (dto.File.Length > 50 * 1024 * 1024)
                throw new Exception("الملف أكبر من 50MB");

            var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(dto.File.FileName).ToLower();
            if (!allowedTypes.Contains(fileExtension))
                throw new Exception("نوع الملف مش مسموح بيه");

            var existingFiles = await _unitOfWork.GetRepository<MedicalFile>()
                .FindAsync(f => f.PatientId == patientId);
            var totalSize = existingFiles.Sum(f => f.FileSizeInBytes);
            if (totalSize + dto.File.Length > 500 * 1024 * 1024)
                throw new Exception("تجاوزت الحد المسموح به 500MB");

            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var medicalFile = new MedicalFile
            {
                PatientId = patientId,
                Category = dto.Category,
                FileName = dto.File.FileName,
                FileUrl = $"/uploads/{uniqueFileName}",
                FileType = fileExtension,
                FileSizeInBytes = dto.File.Length,
                UploadedAtUtc = DateTime.UtcNow,
                OcrStatus = OcrStatus.NotStarted
            };

            await _unitOfWork.GetRepository<MedicalFile>().AddAsync(medicalFile);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(medicalFile);
        }

        public async Task DeleteFileAsync(int fileId, int patientId)
        {
            var file = await _unitOfWork.GetRepository<MedicalFile>().GetByIdAsync(fileId);

            if (file == null || file.PatientId != patientId)
                throw new Exception("الملف مش موجود");

            var filePath = Path.Combine(_uploadsPath, Path.GetFileName(file.FileUrl));
            if (File.Exists(filePath))
                File.Delete(filePath);

            _unitOfWork.GetRepository<MedicalFile>().Delete(file);
            await _unitOfWork.SaveChangesAsync();
        }

        // ============ Doctor Endpoints ============

        public async Task<MedicalFileDto> DoctorUploadFileAsync(
            int patientId, int doctorId, UploadMedicalFileDto dto)
        {
            await EnsureDoctorHasAppointmentWithPatientAsync(patientId, doctorId);

            if (dto.File.Length > 50 * 1024 * 1024)
                throw new Exception("الملف أكبر من 50MB");

            var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(dto.File.FileName).ToLower();
            if (!allowedTypes.Contains(fileExtension))
                throw new Exception("نوع الملف مش مسموح بيه");

            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var medicalFile = new MedicalFile
            {
                PatientId = patientId,
                Category = dto.Category,
                FileName = dto.File.FileName,
                FileUrl = $"/uploads/{uniqueFileName}",
                FileType = fileExtension,
                FileSizeInBytes = dto.File.Length,
                UploadedAtUtc = DateTime.UtcNow,
                OcrStatus = OcrStatus.NotStarted
            };

            await _unitOfWork.GetRepository<MedicalFile>().AddAsync(medicalFile);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(medicalFile);
        }

        public async Task<IEnumerable<MedicalFileDto>> GetFilesForDoctorAsync(
            int patientId, int doctorId)
        {
            await EnsureDoctorHasAppointmentWithPatientAsync(patientId, doctorId);

            var files = await _unitOfWork.GetRepository<MedicalFile>()
                .FindAsync(f => f.PatientId == patientId);

            return files.Select(f => MapToDto(f));
        }

        // ============ Mapper ============

        private MedicalFileDto MapToDto(MedicalFile file) => new MedicalFileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            FileUrl = ImageUrlHelper.ToFullUrl(file.FileUrl),
            FileType = file.FileType,
            FileSizeInBytes = file.FileSizeInBytes,
            Category = file.Category,
            UploadedAtUtc = file.UploadedAtUtc,
            OcrStatus = file.OcrStatus,
            OcrProcessedText = file.OcrProcessedText
        };
    }
}
