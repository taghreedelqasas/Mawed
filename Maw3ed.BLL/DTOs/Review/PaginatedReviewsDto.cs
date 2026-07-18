namespace Maw3ed.BLL.DTOs.Review
{
    public class PaginatedReviewsDto
    {
        public IEnumerable<ReviewResponseDto> Reviews { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
