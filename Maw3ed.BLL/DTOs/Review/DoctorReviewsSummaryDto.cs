namespace Maw3ed.BLL.DTOs.Review
{
    public class DoctorReviewsSummaryDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public IEnumerable<ReviewResponseDto> Reviews { get; set; } = [];
    }
}
