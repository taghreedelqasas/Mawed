using FluentValidation;
using Maw3ed.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Maw3ed.BLL
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        private static readonly string[] AllowedRoles = { "Patient", "Doctor" };
        //private readonly RegisterDto _registerDTO;
        private readonly IUnitOfWork _unitOfWork;
        public RegisterDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;


            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^01[0125][0-9]{8}$")
                .WithMessage("Phone number must be a valid Egyptian mobile number.");

            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
                .WithMessage("Password and confirmation password do not match.");

            RuleFor(x => x.SSN)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Your SSN is Reuired")
                .Length(14).WithMessage("Your SSN must be 14 Number")
                .Matches(@"^[23]\d{13}$").WithMessage("Invalid SSN")

                .MustAsync(async (ssn, cancellation) =>
                {
                    bool isUnique = !await _unitOfWork.AuthRepository.AnyAsync(u => u.SSN == ssn, cancellation);
                    return isUnique;
                }).WithMessage("SSN is Already Existttt.");


            RuleFor(x => x.BirthDate).LessThan(System.DateTime.Now)
                .WithMessage("Birth date must be in the past.");

            RuleFor(x => x.Role).NotEmpty()
                .Must(role => AllowedRoles.Contains(role))
                .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}");

            // ---- Doctor-only fields ----
            When(x => x.Role == "Doctor", () =>
            {
                RuleFor(x => x.LicenseNumber).NotEmpty();
                RuleFor(x => x.Certificate).NotEmpty();
                RuleFor(x => x.ConsultationFee).NotNull().GreaterThan(0);
                RuleFor(x => x.Address).NotEmpty();
                RuleFor(x => x.GraduationDate).NotNull();
                RuleFor(x => x.DepartmentId).NotNull().GreaterThan(0);
            });
        }
    }
}
