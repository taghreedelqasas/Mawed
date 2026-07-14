using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Review;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        /// <summary>
        /// Creates a new review for a doctor. The patient must have a completed appointment with the doctor.
        /// A patient can only review a doctor once.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var result = await _reviewService.CreateReviewAsync(CurrentUserId, dto);

            return result.ToCreatedResult(
                this,
                nameof(GetReviewById),
                new { id = result.Data?.Id },
                new { result.Message, result.Data });
        }

        /// <summary>
        /// Updates an existing review. Only the original author can edit their review.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            var result = await _reviewService.UpdateReviewAsync(CurrentUserId, id, dto);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Deletes a review. Only the original author can delete their review.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Gets all reviews written by the authenticated patient, with pagination.
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(PaginatedReviewsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetMyReviewsAsync(CurrentUserId, page, pageSize);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Gets all reviews for a specific doctor with pagination, average rating, and total count.
        /// </summary>
        /// <param name="doctorId">The doctor's ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet("doctors/{doctorId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DoctorReviewsSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDoctorReviews(
            int doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetDoctorReviewsAsync(doctorId, page, pageSize);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Gets the rating distribution breakdown for a doctor (count per star level).
        /// Returns how many 5-star, 4-star, 3-star, 2-star, and 1-star reviews the doctor has.
        /// </summary>
        /// <param name="doctorId">The doctor's ID</param>
        [HttpGet("doctors/{doctorId}/distribution")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RatingDistributionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRatingDistribution(int doctorId)
        {
            var result = await _reviewService.GetRatingDistributionAsync(doctorId);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Gets a single review by its ID.
        /// </summary>
        /// <param name="id">The review ID</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Admin-only: Deletes any review by ID.
        /// </summary>
        /// <param name="id">The review ID to delete</param>
        [HttpDelete("{id}/admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminDeleteReview(int id)
        {
            var result = await _reviewService.AdminDeleteReviewAsync(id);
            return result.ToActionResult(this);
        }
    }
}
