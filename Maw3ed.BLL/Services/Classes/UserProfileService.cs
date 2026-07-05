using Microsoft.AspNetCore.Hosting;
using Maw3ed.BLL.DTOs.PatientDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{


    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UserProfileService(UserManager<ApplicationUser> userManager,
                IWebHostEnvironment environment)

        {
            _userManager = userManager;
            _environment = environment;

        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("المستخدم مش موجود");

            return new UserProfileDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender == null ? null : (user.Gender == Gender.Male ? "ذكر" : "أنثى"),
                ProfilePictureUrl = user.ProfilePictureUrl 
            };
        }

        public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("المستخدم مش موجود");

            // بس غيري اللي اتبعتلك
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.BirthDate != null) user.BirthDate = dto.BirthDate.Value;
            if (dto.Gender != null) user.Gender = dto.Gender;

            await _userManager.UpdateAsync(user);
        }

        public async Task<string> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("المستخدم مش موجود");

            // تحقق من النوع
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedTypes.Contains(extension))
                throw new Exception("نوع الصورة مش مسموح بيه، بس jpg أو png");

            // تحقق من الحجم — مش أكتر من 5MB
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("الصورة كبيرة أوي، لازم تكون أقل من 5MB");

            // احفظ الصورة
            var basePath = _environment.WebRootPath
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsPath = Path.Combine(basePath, "profile-pictures");

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // لو عنده صورة قديمة، امسحها
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldPath = Path.Combine(basePath, user.ProfilePictureUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/profile-pictures/{fileName}";
            await _userManager.UpdateAsync(user);

            return user.ProfilePictureUrl;
        }
        public async Task DeleteProfilePictureAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("المستخدم مش موجود");

            // لو مفيش صورة أصلاً
            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                throw new Exception("مفيش صورة عشان تمسحها");

            // امسح الصورة من السيرفر
            var basePath = _environment.WebRootPath
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var oldPath = Path.Combine(basePath, user.ProfilePictureUrl.TrimStart('/'));
            if (File.Exists(oldPath))
                File.Delete(oldPath);

            // امسح الرابط من الـ Database
            user.ProfilePictureUrl = null;
            await _userManager.UpdateAsync(user);
        }
    } }

