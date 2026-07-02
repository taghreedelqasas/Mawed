using Maw3ed.BLL.DTOs.MedicalFiles;
using Maw3ed.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IMedicalFileService
    {
        // جيب كل ملفات المريض
        Task<IEnumerable<MedicalFileDto>> GetPatientFilesAsync(int patientId);

        // جيب ملفات المريض حسب النوع
        Task<IEnumerable<MedicalFileDto>> GetPatientFilesByCategoryAsync(int patientId, MedicalFileCategory category);

        // رفع ملف جديد
        Task<MedicalFileDto> UploadFileAsync(int patientId, UploadMedicalFileDto dto);

        // مسح ملف
        Task DeleteFileAsync(int fileId, int patientId);

        // جيب ملف واحد بالـ ID
        Task<MedicalFileDto> GetFileByIdAsync(int fileId, int patientId);

        // جيب عدد الملفات لكل category
        Task<FileCategorySummaryDto> GetCategorySummaryAsync(int patientId);

        Task<MedicalFileDto> DoctorUploadFileAsync(int patientId, int doctorId, UploadMedicalFileDto dto);
        Task<IEnumerable<MedicalFileDto>> GetFilesForDoctorAsync(int patientId, int doctorId);
    }
}
