using FluentValidation;
using Maw3ed.BLL.DTOs.Review;

namespace Maw3ed.BLL.Validators
{
    public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
    {
        public UpdateReviewDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            When(x => x.Comment != null, () =>
            {
                RuleFor(x => x.Comment)
                    .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
            });
        }
    }
}
