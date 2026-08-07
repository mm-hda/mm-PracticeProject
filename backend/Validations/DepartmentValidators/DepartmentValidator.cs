using FluentValidation;

using backend.Dto.DepartmentDtos;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.DepartmentValidators;

public sealed class DepartmentValidator : AbstractValidator<DepartmentDto>
{
    public DepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.DepartmentNameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(3)
            .WithMessage(CustomCodes.DepartmentNameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(100)
            .WithMessage(CustomCodes.DepartmentNameTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
            .WithMessage(CustomCodes.InvalidDepartmentName.ToString(CultureInfo.InvariantCulture));
    }
}
