using FluentValidation;
using Maw3ed.DAL;
using Microsoft.AspNetCore.WebUtilities;
using Maw3ed.DAL.Reposatries.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class AuthManager : IAuthManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenManager _tokenManager;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IEmailService _emailService;

        public AuthManager(
            IUnitOfWork unitOfWork,
            ITokenManager tokenManager,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _tokenManager = tokenManager;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var validation = await _registerValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return AuthResponseDto.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());

            var existingUser = await _unitOfWork.AuthRepository.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return AuthResponseDto.Fail("Email is already registered.");

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                SSN = dto.SSN,
                BirthDate = dto.BirthDate,
                IsActive = true
            };

            var createResult = await _unitOfWork.AuthRepository.CreateUserAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return AuthResponseDto.Fail(createResult.Errors.Select(e => e.Description).ToArray());

            // Make sure the role exists, then assign it.
            if (!await _unitOfWork.AuthRepository.RoleExistsAsync(dto.Role))
                await _unitOfWork.AuthRepository.CreateRoleAsync(dto.Role);

            await _unitOfWork.AuthRepository.AddToRoleAsync(user, dto.Role);

            // Create the linked Patient / Doctor row (1-1 with ApplicationUser).
            if (dto.Role == "Patient")
            {
                await _unitOfWork.GetRepository<Patient>().AddAsync(new Patient
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (dto.Role == "Doctor")
            {
                await _unitOfWork.GetRepository<Doctor>().AddAsync(new Doctor
                {
                    UserId = user.Id,
                    LicenseNumber = dto.LicenseNumber!,
                    Certificate = dto.Certificate,

                    Address = dto.Address!,
                    GraduationDate = dto.GraduationDate.Value,

                    SSNImage = dto.SSNImage,
                    CertificateImage = dto.CertificateImage,
                    LicenseImage = dto.LicenseImage,
                    DepartmentId = dto.DepartmentId!.Value,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            //generate confirmation token and resend
            var rawToken = await _unitOfWork.AuthRepository.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var confirmationLink = $"{dto.ClientBaseUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailConfirmationAsync(
                toEmail: user.Email!,
                toName: $"{user.FirstName} {user.LastName}",
                confirmationLink: confirmationLink);
            //don't return JWT yet - user must confirm email first

            //var roles = await _unitOfWork.AuthRepository.GetRolesAsync(user);
            //var (token, expiresOn) = _tokenManager.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                IsAuthenticated = false,
                UserId = user.Id,
                Email = user.Email,
                //UserName = user.UserName,
                //Roles = roles,
                //Token = token,
                //ExpiresOn = expiresOn
                Errors = new System.Collections.Generic.List<string>
                {
                    "Registration successful . Please Check you Email to cionfirm your account"
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var validation = await _loginValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return AuthResponseDto.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());

            var user = await _unitOfWork.AuthRepository.FindByEmailAsync(dto.Email);
            if (user is null)
                return AuthResponseDto.Fail("Invalid email or password.");

            if (!user.IsActive)
                return AuthResponseDto.Fail("This account has been deactivated.");

            // signinManager confirm lockout,password,Emailconfirmd All in one

            var signInResult = await _unitOfWork.AuthRepository.CheckPasswordSignInAsync(user, dto.Password);
            if (signInResult.IsLockedOut)
                return AuthResponseDto.Fail("Account is locked Try again later");
            if (signInResult.IsNotAllowed)
                return AuthResponseDto.Fail("Please Confirm your Email before you login");
            if (!signInResult.Succeeded)
                return AuthResponseDto.Fail("Invalid Email or password");


            var roles = await _unitOfWork.AuthRepository.GetRolesAsync(user);


            // Block Doctor login until admin sets IsVerified = true.

            if (roles.Contains("Doctor"))
            {
                var doctorsList = await _unitOfWork
         .GetRepository<Maw3ed.DAL.Doctor>()
         .GetAllAsync();

                var doctor = doctorsList.FirstOrDefault(d => d.UserId == user.Id);
                // (تأكدي من اسم الميثود عندك في الـ Generic Repository ممكن يكون GetAsync أو FindAsync)

                if (doctor is not null && !doctor.IsVerified)
                    return AuthResponseDto.Fail("Your request is under review. You will be notified once your account is approved.");
            }


            var (token, expiresOn) = _tokenManager.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles,
                Token = token,
                ExpiresOn = expiresOn
            };
        }

        public async Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _unitOfWork.AuthRepository.FindByIdAsync(dto.UserId);
            if (user is null)
                return AuthResponseDto.Fail("Invalid User");

            if (user.EmailConfirmed)
                return AuthResponseDto.Fail("Email is already confirmed");

            //Decoding token 
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await _unitOfWork.AuthRepository.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return AuthResponseDto.Fail(result.Errors.Select(e => e.Description).ToArray());


            //return JWT after confirmation (no need to login again
            var roles = await _unitOfWork.AuthRepository.GetRolesAsync(user);
            var (token, expiresOn) = _tokenManager.GenerateToken(user, roles);
            return new AuthResponseDto
            {
                IsAuthenticated = true,
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles,
                Token = token,
                ExpiresOn = expiresOn
            };

        }

        //forget password
        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _unitOfWork.AuthRepository.FindByEmailAsync(dto.Email);

            // Always return the same message — don't reveal if email exists or not.
            if (user is null || !user.IsActive)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new List<string>
                    {
                        "If this email is registered, a password reset link has been sent."
                    }
                };

            var rawToken = await _unitOfWork.AuthRepository.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var resetLink = $"{dto.ClientBaseUrl}/api/auth/reset-password?userId={user.Id}&token={encodedToken}";

            await _emailService.SendPasswordResetAsync(
                toEmail: user.Email!,
                toName: $"{user.FirstName} {user.LastName}",
                resetLink: resetLink);

            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Errors = new List<string>
                {
                    "If this email is registered, a password reset link has been sent."
                }
            };
        }

        //ResetPassword
        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return AuthResponseDto.Fail("Password and confirmation password do not match.");

            var user = await _unitOfWork.AuthRepository.FindByIdAsync(dto.UserId);
            if (user is null)
                return AuthResponseDto.Fail("Invalid request.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));

            var result = await _unitOfWork.AuthRepository.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
            if (!result.Succeeded)
                return AuthResponseDto.Fail(result.Errors.Select(e => e.Description).ToArray());

            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Errors = new List<string> { "Password has been reset successfully. You can now log in." }
            };
        }

    }
}