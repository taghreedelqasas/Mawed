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
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;


        public AppointmentService(IUnitOfWork unitOfWork, AppDbContext context, INotificationService notificationService, IPaymentService paymentService)

        {
            _unitOfWork = unitOfWork;
            _context    = context;
             _notificationService = notificationService;
            _paymentService = paymentService;
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

            // ← تحقق إن المريض مش حاجز أكتر من 3 دكاترة النهارده
            var todayBookings = await _context.Appointments
                .CountAsync(a => a.PatientId == patient.Id
                              && a.Status != AppointmentStatus.Cancelled
                              && a.CreatedAt.Date == DateTime.UtcNow.Date);

            if (todayBookings >= 3)
                return new(false,
                    "مش هتقدر تحجز أكتر من 3 مواعيد في اليوم الواحد.",
                    null,
                    ServiceError.BadRequest);

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
                            && a.DoctorAvailability.EndTime > slot.StartTime);

            if (hasConflict)
                return new(false, "You already have an appointment in this time range.", null, ServiceError.Conflict);
            // دور على Appointment ملغي قديم على نفس الـ Slot
            var cancelledAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.DoctorAvailabilityId == dto.DoctorAvailabilityId
                                        && a.Status == AppointmentStatus.Cancelled);

            if (cancelledAppointment != null)
            {
                // حدث الـ Appointment القديم بدل ما تعمل جديد
                cancelledAppointment.PatientId = patient.Id;
                cancelledAppointment.DoctorId = slot.DoctorId;
                cancelledAppointment.Status = AppointmentStatus.Pending;
                cancelledAppointment.Notes = dto.Notes;
                slot.IsBooked = true;

                _unitOfWork.GetRepository<Appointment>().Update(cancelledAppointment);
                _unitOfWork.GetRepository<DoctorAvailability>().Update(slot);
                await _unitOfWork.SaveChangesAsync();

                var updated = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p!.User)
                    .Include(a => a.Doctor).ThenInclude(d => d!.User)
                    .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                    .Include(a => a.DoctorAvailability)
                    .FirstAsync(a => a.Id == cancelledAppointment.Id);

                return new(true, "Appointment booked successfully.",
                    MapToResponse(updated, updated.DoctorAvailability));
            }
            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = slot.DoctorId,
                DoctorAvailabilityId = slot.Id,
                Status = AppointmentStatus.Pending,
                Notes = dto.Notes
            };
            if (!string.IsNullOrWhiteSpace(dto.PatientFullName))
            {
                appointment.Notes = $"{dto.Notes}\n" +
                    $"الاسم: {dto.PatientFullName}\n" +
                    $"التليفون: {dto.PatientPhone}\n" +
                    $"الجنس: {dto.PatientGender}\n" +
                    $"العمر: {dto.PatientAge}";
            }
            slot.IsBooked = true;
            await _unitOfWork.GetRepository<Appointment>().AddAsync(appointment);
            _unitOfWork.GetRepository<DoctorAvailability>().Update(slot);
            await _unitOfWork.SaveChangesAsync();

            var saved = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .Include(a => a.DoctorAvailability)
                .FirstAsync(a => a.Id == appointment.Id);

            // إشعار للمريض
            await _notificationService.SendEmailAsync(
                saved.Patient.UserId,
                "تم حجز موعدك بنجاح",
                $"تم حجز موعدك مع الدكتور {saved.Doctor?.User?.FirstName} {saved.Doctor?.User?.LastName} في {saved.DoctorAvailability.StartTime:dd/MM/yyyy hh:mm tt}",
                saved.Id
            );

            // إشعار للدكتور
            await _notificationService.SendEmailAsync(
                saved.Doctor.UserId,
                "لديك موعد جديد",
                $"قام المريض {saved.Patient?.User?.FirstName} {saved.Patient?.User?.LastName} بحجز موعد في {saved.DoctorAvailability.StartTime:dd/MM/yyyy hh:mm tt}",
                saved.Id
            );

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

            // ← تحقق إن المريض مش لغى أكتر من 5 مرات الشهر ده
            var cancelledThisMonth = await _context.Appointments
     .CountAsync(a => a.PatientId == patient.Id
                   && a.Status == AppointmentStatus.Cancelled
                   && a.UpdatedAt.HasValue
                   && a.UpdatedAt.Value.Month == DateTime.UtcNow.Month
                   && a.UpdatedAt.Value.Year == DateTime.UtcNow.Year);

            if (cancelledThisMonth >= 5)
                return new(false,
                    "مش هتقدر تلغي أكتر من 5 مواعيد في الشهر الواحد.",
                    ServiceError.BadRequest);

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

            var cancelledAppt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.DoctorAvailability)
                .FirstAsync(a => a.Id == appointmentId);

            // إشعار للمريض
            // إشعار للمريض
            await _notificationService.SendEmailAsync(
                cancelledAppt.Patient.UserId,
                "تم إلغاء موعدك",
                $"تم إلغاء موعدك مع الدكتور {cancelledAppt.Doctor?.User?.FirstName} في {cancelledAppt.DoctorAvailability.StartTime:dd/MM/yyyy hh:mm tt}",
                appointmentId
            );

            // ── استرداد المبلغ لو الموعد كان مدفوع وتم الإلغاء قبل 24 ساعة ──
            string refundNote = "";

            if (appointment.PaymentStatus == PaymentStatus.Paid)
            {
                var hoursUntilAppointment = (appointment.DoctorAvailability.StartTime - DateTime.UtcNow).TotalHours;

                if (hoursUntilAppointment >= 24)
                {
                    var refundResult = await _paymentService.RefundAppointmentPaymentAsync(appointment.Id);
                    refundNote = refundResult.Success
                        ? " وتم استرداد المبلغ المدفوع بنجاح."
                        : " لكن حصلت مشكلة أثناء استرداد المبلغ، هيتم التعامل معاها يدويًا من فريق الدعم.";
                }
                else
                {
                    refundNote = " ملحوظة: الإلغاء تم بعد أقل من 24 ساعة من الموعد، فمينفعش يترد المبلغ المدفوع.";
                }
            }

            return new(true, "Appointment cancelled successfully." + refundNote);
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
            // امسحي الـ Appointment الملغي القديم على الـ Slot الجديد لو موجود
            var oldCancelledOnNewSlot = await _context.Appointments
                .FirstOrDefaultAsync(a => a.DoctorAvailabilityId == dto.NewDoctorAvailabilityId
                                        && a.Status == AppointmentStatus.Cancelled);

            if (oldCancelledOnNewSlot != null)
                _context.Appointments.Remove(oldCancelledOnNewSlot);
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

            // بعد SaveChangesAsync أضيفي
            var confirmedAppt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.DoctorAvailability)
                .FirstAsync(a => a.Id == appointmentId);

            await _notificationService.SendEmailAsync(
                confirmedAppt.Patient.UserId,
                "تم تأكيد موعدك",
                $"تم تأكيد موعدك مع الدكتور {confirmedAppt.Doctor?.User?.FirstName} في {confirmedAppt.DoctorAvailability.StartTime:dd/MM/yyyy hh:mm tt}",
                appointmentId
            );

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
                PatientImageUrl = a.Patient?.User?.ProfilePictureUrl ?? string.Empty,
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
