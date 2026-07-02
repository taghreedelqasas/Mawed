using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Appointment;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public AppointmentService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context    = context;
        }

        // ── Get Available Slots ──────────────────────────────────────────
        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId)
        {
            var doctorExists = await _context.Doctors
                .AnyAsync(d => d.Id == doctorId && d.IsVerified);

            if (!doctorExists)
                return Enumerable.Empty<AvailableSlotDto>();

            return await _context.DoctorAvailabilities
                .Where(s => s.DoctorId == doctorId
                         && !s.IsBooked
                         && s.StartTime > DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .Select(s => new AvailableSlotDto
                {
                    AvailabilityId = s.Id,
                    StartTime      = s.StartTime,
                    EndTime        = s.EndTime
                })
                .ToListAsync();
        }

        // ── Book Appointment ─────────────────────────────────────────────
        public async Task<ServiceResult<AppointmentResponseDto>>
            BookAppointmentAsync(string patientUserId, BookAppointmentDto dto)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", null, ServiceError.NotFound);

            var slot = await _context.DoctorAvailabilities
                .Include(s => s.Doctor).ThenInclude(d => d!.User)
                .Include(s => s.Doctor).ThenInclude(d => d!.Department)
                .FirstOrDefaultAsync(s => s.Id == dto.DoctorAvailabilityId);

            if (slot is null)
                return new(false, "Slot not found.", null, ServiceError.NotFound);

            if (!slot.Doctor.IsVerified)
                return new(false, "Cannot book with an unverified doctor.", null, ServiceError.BadRequest);

            if (slot.IsBooked)
                return new(false, "This slot is already booked.", null, ServiceError.Conflict);

            if (slot.StartTime <= DateTime.UtcNow)
                return new(false, "Cannot book a past slot.", null, ServiceError.BadRequest);

            bool hasConflict = await _context.Appointments
                .Include(a => a.DoctorAvailability)
                .AnyAsync(a => a.PatientId == patient.Id
                            && a.Status != AppointmentStatus.Cancelled
                            && a.DoctorAvailability.StartTime < slot.EndTime
                            && a.DoctorAvailability.EndTime   > slot.StartTime);

            if (hasConflict)
                return new(false, "You already have an appointment in this time range.", null, ServiceError.Conflict);

            var appointment = new Appointment
            {
                PatientId            = patient.Id,
                DoctorId             = slot.DoctorId,
                DoctorAvailabilityId = slot.Id,
                Status               = AppointmentStatus.Pending,
                Notes                = dto.Notes
            };

            slot.IsBooked = true;
            await _unitOfWork.GetRepository<Appointment>().AddAsync(appointment);
            _unitOfWork.GetRepository<DoctorAvailability>().Update(slot);
            await _unitOfWork.SaveChangesAsync();

            // FIX: بدل ما نعمل Reference loading على entity قد تكون null
            // نعمل query واحدة بعد الـ save بالـ Id المتولد
            var saved = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .Include(a => a.DoctorAvailability)
                .FirstAsync(a => a.Id == appointment.Id);

            return new(true, "Appointment booked successfully.", MapToResponse(saved, saved.DoctorAvailability));
        }

        // ── Cancel Appointment ───────────────────────────────────────────
        public async Task<ServiceResult>
            CancelAppointmentAsync(string patientUserId, int appointmentId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", ServiceError.NotFound);

            var appointment = await _context.Appointments
                .Include(a => a.DoctorAvailability)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

            if (appointment is null)
                return new(false, "Appointment not found.", ServiceError.NotFound);

            if (appointment.Status == AppointmentStatus.Cancelled)
                return new(false, "Appointment is already cancelled.", ServiceError.Conflict);

            if (appointment.Status == AppointmentStatus.Completed)
                return new(false, "Cannot cancel a completed appointment.", ServiceError.BadRequest);

            if (appointment.DoctorAvailability.StartTime <= DateTime.UtcNow.AddHours(1))
                return new(false, "Cannot cancel less than 1 hour before appointment.", ServiceError.BadRequest);

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.DoctorAvailability.IsBooked = false;

            _unitOfWork.GetRepository<Appointment>().Update(appointment);
            _unitOfWork.GetRepository<DoctorAvailability>().Update(appointment.DoctorAvailability);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Appointment cancelled successfully.");
        }

        // ── Reschedule Appointment ───────────────────────────────────────
        public async Task<ServiceResult<AppointmentResponseDto>>
            RescheduleAppointmentAsync(string patientUserId, int appointmentId, RescheduleAppointmentDto dto)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", null, ServiceError.NotFound);

            var appointment = await _context.Appointments
                .Include(a => a.DoctorAvailability)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

            if (appointment is null)
                return new(false, "Appointment not found.", null, ServiceError.NotFound);

            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
                return new(false, "Cannot reschedule this appointment.", null, ServiceError.BadRequest);

            if (appointment.DoctorAvailabilityId == dto.NewDoctorAvailabilityId)
                return new(false, "New slot is the same as the current slot.", null, ServiceError.BadRequest);

            var newSlot = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(s => s.Id == dto.NewDoctorAvailabilityId
                                       && s.DoctorId == appointment.DoctorId
                                       && !s.IsBooked
                                       && s.StartTime > DateTime.UtcNow);

            if (newSlot is null)
                return new(false, "New slot is not available.", null, ServiceError.NotFound);

            var oldSlot = appointment.DoctorAvailability;
            oldSlot.IsBooked  = false;
            newSlot.IsBooked  = true;
            appointment.DoctorAvailabilityId = newSlot.Id;
            appointment.Status               = AppointmentStatus.Pending;

            _unitOfWork.GetRepository<Appointment>().Update(appointment);
            _unitOfWork.GetRepository<DoctorAvailability>().Update(oldSlot);
            _unitOfWork.GetRepository<DoctorAvailability>().Update(newSlot);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Appointment rescheduled successfully.", MapToResponse(appointment, newSlot));
        }

        // ── Get Patient Appointments ─────────────────────────────────────
        public async Task<IEnumerable<AppointmentResponseDto>>
            GetPatientAppointmentsAsync(string patientUserId)
        {
            var list = await _context.Appointments
                .Include(a => a.DoctorAvailability)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Where(a => a.Patient.UserId == patientUserId)
                .OrderByDescending(a => a.DoctorAvailability.StartTime)
                .ToListAsync();

            return list.Select(a => MapToResponse(a, a.DoctorAvailability));
        }

        // ── Get Doctor Appointments ──────────────────────────────────────
        public async Task<IEnumerable<AppointmentResponseDto>>
            GetDoctorAppointmentsAsync(string doctorUserId)
        {
            var list = await _context.Appointments
                .Include(a => a.DoctorAvailability)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Where(a => a.Doctor.UserId == doctorUserId)
                .OrderByDescending(a => a.DoctorAvailability.StartTime)
                .ToListAsync();

            return list.Select(a => MapToResponse(a, a.DoctorAvailability));
        }

        // ── Doctor: Confirm ──────────────────────────────────────────────
        public async Task<ServiceResult>
            ConfirmAppointmentAsync(string doctorUserId, int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId
                                       && a.Doctor.UserId == doctorUserId);

            if (appointment is null)
                return new(false, "Appointment not found.", ServiceError.NotFound);

            if (appointment.Status != AppointmentStatus.Pending)
                return new(false, $"Appointment is already {appointment.Status}.", ServiceError.Conflict);

            appointment.Status = AppointmentStatus.Confirmed;
            _unitOfWork.GetRepository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Appointment confirmed.");
        }

        // ── Doctor: Complete ─────────────────────────────────────────────
        public async Task<ServiceResult>
            CompleteAppointmentAsync(string doctorUserId, int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId
                                       && a.Doctor.UserId == doctorUserId);

            if (appointment is null)
                return new(false, "Appointment not found.", ServiceError.NotFound);

            if (appointment.Status != AppointmentStatus.Confirmed)
                return new(false, "Only confirmed appointments can be completed.", ServiceError.BadRequest);

            appointment.Status = AppointmentStatus.Completed;
            _unitOfWork.GetRepository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Appointment marked as completed.");
        }

        // ── Mapper ───────────────────────────────────────────────────────
        private static AppointmentResponseDto MapToResponse(Appointment a, DoctorAvailability slot)
        {
            return new AppointmentResponseDto
            {
                Id              = a.Id,
                Status          = a.Status.ToString(),
                Notes           = a.Notes,
                PatientId       = a.PatientId,
                PatientName     = $"{a.Patient?.User?.FirstName} {a.Patient?.User?.LastName}",
                DoctorId        = a.DoctorId,
                DoctorName      = $"{a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}",
                DoctorSpecialty = a.Doctor?.Department?.Name ?? string.Empty,
                SlotStart       = slot.StartTime,
                SlotEnd         = slot.EndTime,
                CreatedAt       = a.CreatedAt
            };
        }
    }
}
