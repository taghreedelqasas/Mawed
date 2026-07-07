using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maw3ed.BLL.AI.DTOs;

namespace Maw3ed.BLL.AI.Interfaces
{
    public interface IMedicalImageService
    {
        Task<AnalyzeImageResponse> AnalyzeImageAsync(
            Stream imageStream,
            string fileName,
            int? patientId);
    }
}
