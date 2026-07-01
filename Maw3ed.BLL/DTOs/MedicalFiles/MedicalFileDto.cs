using Maw3ed.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.MedicalFiles
{
    
        public class MedicalFileDto
        {
            public int Id { get; set; }
            public string FileName { get; set; }
            public string FileUrl { get; set; }
            public string FileType { get; set; }
            public long FileSizeInBytes { get; set; }
            public MedicalFileCategory Category { get; set; }
            public DateTime UploadedAtUtc { get; set; }
            public OcrStatus OcrStatus { get; set; }
            public string? OcrProcessedText { get; set; }
        }
    }

