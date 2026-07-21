using FluentValidation;

using System.Globalization;

using backend.Dto;
using backend.Validations.CommonValidators;
using backend.GenericResponse;

namespace backend.Validations.AuthValidators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
        .Must(x => !string.IsNullOrWhiteSpace(x))
          .NotEmpty().WithMessage(CustomCodes.InputsNotFound.ToString(CultureInfo.InvariantCulture))
          .ValidCompanyEmail().WithMessage(CustomCodes.InvalidEmail.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage(CustomCodes.PasswordTooShort.ToString(CultureInfo.InvariantCulture))
            .MaximumLength(25).WithMessage(CustomCodes.PasswordTooLong.ToString(CultureInfo.InvariantCulture))
            .StrongPassword().WithMessage(CustomCodes.PasswordNotStrong.ToString(CultureInfo.InvariantCulture));
    }
}
