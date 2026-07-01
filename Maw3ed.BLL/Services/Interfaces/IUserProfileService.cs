using Maw3ed.BLL.DTOs.PatientDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<string> UploadProfilePictureAsync(string userId, IFormFile file);
        Task DeleteProfilePictureAsync(string userId);

    }
}
