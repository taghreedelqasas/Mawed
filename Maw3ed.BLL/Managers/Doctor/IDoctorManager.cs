using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public interface IDoctorManager
    {
        // Returns all doctors whose IsVerified = false (pending admin approval).
        Task<List<DoctorPendingDto>> GetPendingDoctorsAsync();

        // Admin approves a doctor by UserId.
        // Returns false if doctor not found or already verified.
        Task<(bool Success, string Message)> ApproveDoctorAsync(string userId);
    }
}
