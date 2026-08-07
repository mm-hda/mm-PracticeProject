using FluentValidation;

using backend.Dto.ProjectDtos;
using backend.GenericResponse;
using backend.Validations.CommonValidators;

using System.Globalization;

namespace backend.Validations.ProjectValidators;

public sealed class ProjectValidator : AbstractValidator<ProjectDto>
{
    public ProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CustomCodes.ProjectNameRequired.ToString(CultureInfo.InvariantCulture))

            .MinimumLength(3)
            .WithMessage(CustomCodes.ProjectNameTooShort.ToString(CultureInfo.InvariantCulture))

            .MaximumLength(150)
            .WithMessage(CustomCodes.ProjectNameTooLong.ToString(CultureInfo.InvariantCulture))

            .NoMultipleSpaces()
            .WithMessage(CustomCodes.InvalidNameFormat.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture))

            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
            .WithMessage(CustomCodes.InvalidProjectName.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(CustomCodes.DescriptionTooLong.ToString(CultureInfo.InvariantCulture))

            .NoSqlInjection()
            .WithMessage(CustomCodes.SqlInjectionDetected.ToString(CultureInfo.InvariantCulture))

            .NoHtml()
            .WithMessage(CustomCodes.HtmlDetected.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage(CustomCodes.StartDateRequired.ToString(CultureInfo.InvariantCulture))

            .LessThanOrEqualTo(DateTime.UtcNow <= DateTime.UtcNow.AddYears(1) ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow)
            .WithMessage(CustomCodes.InvalidStartDate.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.EndDate)
            .Must((dto, endDate) =>
            {
                if (endDate == null)
                {
                    return true;
                }

                return endDate > dto.StartDate;
            }).WithMessage(CustomCodes.InvalidEndDate.ToString(CultureInfo.InvariantCulture));

        RuleFor(x => x.ProjectManagerId)
            .Must(id => id != Guid.Empty)
            .WithMessage(CustomCodes.ProjectManagerNotFound.ToString(CultureInfo.InvariantCulture));
    }
}
