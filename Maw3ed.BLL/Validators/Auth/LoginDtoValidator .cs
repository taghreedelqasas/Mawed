using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    internal class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}