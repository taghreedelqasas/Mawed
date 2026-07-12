using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorSearchResultDto>> SearchDoctorsAsync(DoctorSearchFilterDto filter);
        Task<DoctorProfileDto?> GetDoctorProfileAsync(int doctorId);
    }
}
