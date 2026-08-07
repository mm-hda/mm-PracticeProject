using FluentValidation;

using backend.Dto.BranchDtos;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.BranchValidators;

public sealed class BranchValidator : AbstractValidator<BranchDto>
{
    public BranchValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.BranchNameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(3)
            .WithMessage(CustomCodes.BranchNameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(100)
            .WithMessage(CustomCodes.BranchNameTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
            .WithMessage(CustomCodes.InvalidBranchName.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage(CustomCodes.LocationRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(3)
            .WithMessage(CustomCodes.LocationTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(200)
            .WithMessage(CustomCodes.LocationTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidLocationFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture));
    }
}
