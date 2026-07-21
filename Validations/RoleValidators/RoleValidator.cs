using FluentValidation;

using backend.Dto.RoleDtos;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.RoleValidators;

public sealed class RoleValidator
    : AbstractValidator<RoleDto>
{
    public RoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.RoleNameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(2)
            .WithMessage(CustomCodes.RoleNameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(50)
            .WithMessage(CustomCodes.RoleNameTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
            .WithMessage(CustomCodes.InvalidRoleName.ToString(CultureInfo.InvariantCulture));
    }
}
