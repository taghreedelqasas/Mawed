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

        // POST api/reviews  → 201 | 403 | 404 | 409
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _reviewService.CreateReviewAsync(CurrentUserId, dto);

            return result.ToCreatedResult(
                this,
                nameof(GetReviewById),
                new { id = result.Data?.Id },
                new { result.Message, result.Data });
        }

        // PUT api/reviews/{id}  → 200 | 403 | 404
        [HttpPut("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _reviewService.UpdateReviewAsync(CurrentUserId, id, dto);
            return result.ToActionResult(this);
        }

        // DELETE api/reviews/{id}  → 200 | 403 | 404
        [HttpDelete("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(CurrentUserId, id);
            return result.ToActionResult(this);
        }

        // GET api/reviews/my  → 200
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyReviews()
        {
            var result = await _reviewService.GetMyReviewsAsync(CurrentUserId);
            return Ok(result);
        }

        // GET api/reviews/doctors/{doctorId}  → 200 | 404
        [HttpGet("doctors/{doctorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorReviews(int doctorId)
        {
            var result = await _reviewService.GetDoctorReviewsAsync(doctorId);
            if (result is null) return NotFound(new { message = "Doctor not found." });
            return Ok(result);
        }

        // GET api/reviews/{id}  → 200 | 404
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);
            if (result is null) return NotFound(new { message = "Review not found." });
            return Ok(result);
        }

        // DELETE api/reviews/{id}/admin  → 200 | 404
        [HttpDelete("{id}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteReview(int id)
        {
            var result = await _reviewService.AdminDeleteReviewAsync(id);
            return result.ToActionResult(this);
        }
    }
}
