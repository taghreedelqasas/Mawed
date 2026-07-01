using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Review;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public ReviewService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context    = context;
        }

        // ── Create Review ────────────────────────────────────────────────
        public async Task<ServiceResult<ReviewResponseDto>>
            CreateReviewAsync(string patientUserId, CreateReviewDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", null, ServiceError.NotFound);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.IsVerified);
            if (doctor is null)
                return new(false, "Doctor not found.", null, ServiceError.NotFound);

            bool hasCompleted = await _context.Appointments
                .AnyAsync(a => a.PatientId == patient.Id
                            && a.DoctorId  == doctor.Id
                            && a.Status    == AppointmentStatus.Completed);
            if (!hasCompleted)
                return new(false, "You can only review a doctor after a completed appointment.", null, ServiceError.Forbidden);

            bool alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.PatientId == patient.Id && r.DoctorId == doctor.Id);
            if (alreadyReviewed)
                return new(false, "You have already reviewed this doctor.", null, ServiceError.Conflict);

            var review = new Review
            {
                PatientId  = patient.Id,
                DoctorId   = doctor.Id,
                Rating     = dto.Rating,
                Comment    = dto.Comment,
                ReviewDate = DateTime.UtcNow
            };

            _unitOfWork.GetReposatry<Review>().Add(review);
            await _unitOfWork.SaveChangesAsync();   // FIX: async

            return new(true, "Review submitted successfully.", MapToResponse(review, patient, doctor));
        }

        // ── Update Review ────────────────────────────────────────────────
        public async Task<ServiceResult<ReviewResponseDto>>
            UpdateReviewAsync(string patientUserId, int reviewId, UpdateReviewDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", null, ServiceError.NotFound);

            var review = await _context.Reviews
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .Include(r => r.Doctor).ThenInclude(d => d!.Department)
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return new(false, "Review not found.", null, ServiceError.NotFound);

            if (review.PatientId != patient.Id)
                return new(false, "You are not allowed to edit this review.", null, ServiceError.Forbidden);

            review.Rating  = dto.Rating;
            review.Comment = dto.Comment;
            _unitOfWork.GetReposatry<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync();   // FIX: async

            return new(true, "Review updated successfully.", MapToResponse(review, review.Patient, review.Doctor));
        }

        // ── Delete Review (Patient) ──────────────────────────────────────
        public async Task<ServiceResult>
            DeleteReviewAsync(string patientUserId, int reviewId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);
            if (patient is null)
                return new(false, "Patient profile not found.", ServiceError.NotFound);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return new(false, "Review not found.", ServiceError.NotFound);

            if (review.PatientId != patient.Id)
                return new(false, "You are not allowed to delete this review.", ServiceError.Forbidden);

            _unitOfWork.GetReposatry<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync();   // FIX: async

            return new(true, "Review deleted successfully.");
        }

        // ── Get My Reviews ───────────────────────────────────────────────
        public async Task<IEnumerable<ReviewResponseDto>>
            GetMyReviewsAsync(string patientUserId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .Include(r => r.Doctor).ThenInclude(d => d!.Department)
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .Where(r => r.Patient.UserId == patientUserId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return reviews.Select(r => MapToResponse(r, r.Patient, r.Doctor));
        }

        // ── Get Doctor Reviews + Average ─────────────────────────────────
        public async Task<DoctorReviewsSummaryDto?>
            GetDoctorReviewsAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsVerified);

            if (doctor is null) return null;

            var reviews = await _context.Reviews
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return new DoctorReviewsSummaryDto
            {
                DoctorId      = doctor.Id,
                DoctorName    = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews  = reviews.Count,
                Reviews       = reviews.Select(r => MapToResponse(r, r.Patient, doctor))
            };
        }

        // ── Get Review By Id ─────────────────────────────────────────────
        public async Task<ReviewResponseDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .Include(r => r.Doctor).ThenInclude(d => d!.Department)
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null) return null;
            return MapToResponse(review, review.Patient, review.Doctor);
        }

        // ── Admin Delete ─────────────────────────────────────────────────
        public async Task<ServiceResult> AdminDeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return new(false, "Review not found.", ServiceError.NotFound);

            _unitOfWork.GetReposatry<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync();   // FIX: async

            return new(true, "Review deleted by admin.");
        }

        // ── Mapper ───────────────────────────────────────────────────────
        private static ReviewResponseDto MapToResponse(Review r, Patient patient, Doctor doctor)
        {
            return new ReviewResponseDto
            {
                Id              = r.Id,
                Rating          = r.Rating,
                Comment         = r.Comment,
                ReviewDate      = r.ReviewDate,
                PatientId       = r.PatientId,
                PatientName     = $"{patient?.User?.FirstName} {patient?.User?.LastName}",
                DoctorId        = r.DoctorId,
                DoctorName      = $"{doctor?.User?.FirstName} {doctor?.User?.LastName}",
                DoctorSpecialty = doctor?.Department?.Name ?? string.Empty
            };
        }
    }
}
