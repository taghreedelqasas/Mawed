using FluentValidation;
using Maw3ed.BLL.DTOs.PatientDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.APIs.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IValidator<UpdateProfileDto> _validator;

        public UserProfileController(
            IUserProfileService userProfileService,
            IValidator<UpdateProfileDto> validator)
        {
            _userProfileService = userProfileService;
            _validator = validator;
        }

        // GET: api/UserProfile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var profile = await _userProfileService.GetProfileAsync(userId);
            return Ok(profile);
        }

        // PATCH: api/UserProfile
        [HttpPatch]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            await _userProfileService.UpdateProfileAsync(userId, dto);
            return Ok("تم تحديث البيانات");
        }

        // POST: api/UserProfile/picture
        [HttpPost("picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var url = await _userProfileService.UploadProfilePictureAsync(userId, file);
                return Ok(new { ProfilePictureUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/UserProfile/picture
        [HttpDelete("picture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                await _userProfileService.DeleteProfilePictureAsync(userId);
                return Ok("تم مسح الصورة");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
