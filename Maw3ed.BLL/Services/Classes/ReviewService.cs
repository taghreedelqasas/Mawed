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
                Comment    = dto.Comment?.Trim(),
                ReviewDate = DateTime.UtcNow
            };

             await _unitOfWork.GetRepository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

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

            review.Rating     = dto.Rating;
            review.Comment    = dto.Comment?.Trim();
            review.ReviewDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

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

            _unitOfWork.GetRepository<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Review deleted successfully.");
        }

        // ── Get My Reviews (Paginated) ───────────────────────────────────
        public async Task<ServiceResult<PaginatedReviewsDto>>
            GetMyReviewsAsync(string patientUserId, int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .Include(r => r.Doctor).ThenInclude(d => d!.Department)
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .Where(r => r.Patient.UserId == patientUserId)
                .OrderByDescending(r => r.ReviewDate);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PaginatedReviewsDto
            {
                Reviews     = reviews.Select(r => MapToResponse(r, r.Patient, r.Doctor)),
                TotalCount  = totalCount,
                Page        = page,
                PageSize    = pageSize,
                TotalPages  = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return new(true, "Reviews retrieved successfully.", result);
        }

        // ── Get Doctor Reviews + Average (Paginated) ─────────────────────
        public async Task<ServiceResult<DoctorReviewsSummaryDto>>
            GetDoctorReviewsAsync(int doctorId, int page, int pageSize)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsVerified);

            if (doctor is null)
                return new(false, "Doctor not found.", null, ServiceError.NotFound);

            var query = _context.Reviews
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.ReviewDate);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var avgRating = totalCount > 0
                ? Math.Round(await query.AverageAsync(r => r.Rating), 1)
                : (double?)null;

            var summary = new DoctorReviewsSummaryDto
            {
                DoctorId      = doctor.Id,
                DoctorName    = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                AverageRating = avgRating,
                TotalReviews  = totalCount,
                Reviews       = reviews.Select(r => MapToResponse(r, r.Patient, doctor))
            };

            return new(true, "Doctor reviews retrieved successfully.", summary);
        }

        // ── Get Review By Id ─────────────────────────────────────────────
        public async Task<ServiceResult<ReviewResponseDto>> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Doctor).ThenInclude(d => d!.User)
                .Include(r => r.Doctor).ThenInclude(d => d!.Department)
                .Include(r => r.Patient).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return new(false, "Review not found.", null, ServiceError.NotFound);

            return new(true, "Review retrieved successfully.", MapToResponse(review, review.Patient, review.Doctor));
        }

        // ── Admin Delete ─────────────────────────────────────────────────
        public async Task<ServiceResult> AdminDeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return new(false, "Review not found.", ServiceError.NotFound);

            _unitOfWork.GetRepository<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Review deleted by admin.");
        }

        // ── Get Rating Distribution ──────────────────────────────────────
        public async Task<ServiceResult<RatingDistributionDto>>
            GetRatingDistributionAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsVerified);

            if (doctor is null)
                return new(false, "Doctor not found.", null, ServiceError.NotFound);

            var reviews = await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();

            var total = reviews.Count;

            var distribution = new RatingDistributionDto
            {
                DoctorId      = doctor.Id,
                DoctorName    = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                AverageRating = total > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : null,
                TotalReviews  = total,
                FiveStar      = reviews.Count(r => r.Rating == 5),
                FourStar      = reviews.Count(r => r.Rating == 4),
                ThreeStar     = reviews.Count(r => r.Rating == 3),
                TwoStar       = reviews.Count(r => r.Rating == 2),
                OneStar       = reviews.Count(r => r.Rating == 1)
            };

            return new(true, "Rating distribution retrieved successfully.", distribution);
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
