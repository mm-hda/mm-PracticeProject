using FluentValidation;

using backend.Dto.PositionDtos;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.PositionValidators;

public sealed class PositionDtoValidator
    : AbstractValidator<PositionDto>
{
    public PositionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.PositionNameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(3)
            .WithMessage(CustomCodes.PositionNameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(100)
            .WithMessage(CustomCodes.PositionNameTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
            .WithMessage(CustomCodes.InvalidPositionName.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage(CustomCodes.DepartmentNotFound.ToString(CultureInfo.InvariantCulture))

            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.DepartmentNotFound.ToString(CultureInfo.InvariantCulture));
    }
}
