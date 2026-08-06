using FluentValidation;

using backend.Dto;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.AuthValidators;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.NameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(5)
            .WithMessage(CustomCodes.NameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(100)
            .WithMessage(CustomCodes.NameTooLong.ToString(CultureInfo.InvariantCulture))

            .FullName()
            .WithMessage(CustomCodes.FullNameRequired.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.Email ?? "")
            .NotEmpty()
            .WithMessage(CustomCodes.EmailRequired.ToString(CultureInfo.InvariantCulture))

            .ValidCompanyEmail()
            .WithMessage(CustomCodes.InvalidEmail.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(100)
            .WithMessage(CustomCodes.EmailTooLong.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(CustomCodes.PasswordRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(8)
            .WithMessage(CustomCodes.PasswordTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(25)
            .WithMessage(CustomCodes.PasswordTooLong.ToString(CultureInfo.InvariantCulture))

            .StrongPassword()
            .WithMessage(CustomCodes.InvalidPasswordFormat.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.DOB)
            .NotNull()
            .WithMessage(CustomCodes.DOBRequired.ToString(CultureInfo.InvariantCulture))

            .Must(BeValidAge)
            .WithMessage(CustomCodes.InvalidAge.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.BranchId)
            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.BranchNotFound.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.DepartmentNotFound.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.PositionId)
            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.PositionNotFound.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.RoleId)
            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.RoleNotFound.ToString(CultureInfo.InvariantCulture));
    }

    private static bool BeValidAge(DateTime? dob)
    {
        if (dob == null)
        {
            return false;
        }
        var age = DateTime.UtcNow.Year - dob.Value.Year;

        if (dob.Value.Date > DateTime.UtcNow.AddYears(-age))
        {
            age--;
        }

        var results = age is >= 18 and <= 70;
        return results;
    }
}
