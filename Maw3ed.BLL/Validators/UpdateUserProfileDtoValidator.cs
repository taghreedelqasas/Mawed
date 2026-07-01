using FluentValidation;
using Maw3ed.BLL.DTOs.PatientDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Validators
{
    public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateUserProfileDtoValidator()
        {
            // لو بعت FirstName متكونش فاضية
            When(x => x.FirstName != null, () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("الاسم الأول مش ممكن يكون فاضي")
                    .MinimumLength(2).WithMessage("الاسم الأول لازم يكون على الأقل حرفين")
                    .MaximumLength(50).WithMessage("الاسم الأول طويل أوي");
            });

            When(x => x.LastName != null, () =>
            {
                RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("الاسم الأخير مش ممكن يكون فاضي")
                    .MinimumLength(2).WithMessage("الاسم الأخير لازم يكون على الأقل حرفين")
                    .MaximumLength(50).WithMessage("الاسم الأخير طويل أوي");
            });

            When(x => x.PhoneNumber != null, () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("رقم التليفون مش ممكن يكون فاضي")
                    .Matches(@"^01[0125][0-9]{8}$").WithMessage("رقم التليفون غلط");
            });

            When(x => x.BirthDate != null, () =>
            {
                RuleFor(x => x.BirthDate)
                    .LessThan(DateTime.UtcNow).WithMessage("تاريخ الميلاد غلط")
                    .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("تاريخ الميلاد غلط");
            });

            When(x => x.Gender != null, () =>
            {
                RuleFor(x => x.Gender)
                    .IsInEnum().WithMessage("الجنس غلط");
            });
        }
    }
}
