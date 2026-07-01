using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Appointment;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId);

        Task<ServiceResult<AppointmentResponseDto>> BookAppointmentAsync(
            string patientUserId, BookAppointmentDto dto);

        Task<ServiceResult> CancelAppointmentAsync(
            string patientUserId, int appointmentId);

        Task<ServiceResult<AppointmentResponseDto>> RescheduleAppointmentAsync(
            string patientUserId, int appointmentId, RescheduleAppointmentDto dto);

        Task<IEnumerable<AppointmentResponseDto>> GetPatientAppointmentsAsync(string patientUserId);

        Task<IEnumerable<AppointmentResponseDto>> GetDoctorAppointmentsAsync(string doctorUserId);

        Task<ServiceResult> ConfirmAppointmentAsync(string doctorUserId, int appointmentId);

        Task<ServiceResult> CompleteAppointmentAsync(string doctorUserId, int appointmentId);
    }
}
