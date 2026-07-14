using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Review;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResult<ReviewResponseDto>> CreateReviewAsync(
            string patientUserId, CreateReviewDto dto);

        Task<ServiceResult<ReviewResponseDto>> UpdateReviewAsync(
            string patientUserId, int reviewId, UpdateReviewDto dto);

        Task<ServiceResult> DeleteReviewAsync(string patientUserId, int reviewId);

        Task<ServiceResult<PaginatedReviewsDto>> GetMyReviewsAsync(
            string patientUserId, int page, int pageSize);

        Task<ServiceResult<DoctorReviewsSummaryDto>> GetDoctorReviewsAsync(
            int doctorId, int page, int pageSize);

        Task<ServiceResult<ReviewResponseDto>> GetReviewByIdAsync(int reviewId);

        Task<ServiceResult> AdminDeleteReviewAsync(int reviewId);

        Task<ServiceResult<RatingDistributionDto>> GetRatingDistributionAsync(int doctorId);
    }
}
