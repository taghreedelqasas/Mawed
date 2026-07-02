using Maw3ed.DAL.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.MedicalFiles
{
    public class UploadMedicalFileDto
    {
        public IFormFile File { get; set; }
        public MedicalFileCategory Category { get; set; }
    }
}
