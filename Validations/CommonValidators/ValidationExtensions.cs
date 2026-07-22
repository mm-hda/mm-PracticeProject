using FluentValidation;

namespace backend.Validations.CommonValidators;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder.Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$");

    public static IRuleBuilderOptions<T, string> ValidCompanyEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(email =>
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var emailParts = email.Split('@');

            if (emailParts.Length != 2)
            {
                return false;
            }
            var domain = emailParts[1];

            var domainParts = domain.Contains('.', StringComparison.Ordinal);

            return domainParts;
        });
    }

    public static IRuleBuilderOptions<T, string> FullName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(name =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            return name.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length >= 2;
        });
    }

    public static IRuleBuilderOptions<T, string> NoMultipleSpaces<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(text =>
         {
             if (string.IsNullOrWhiteSpace(text))
             {
                 return true;
             }
             var result = text.Contains("  ", StringComparison.Ordinal);
             return !result;
         });
    }

    private static readonly string[] Keywords =
    [
        "SELECT",
        "INSERT",
        "UPDATE",
        "DELETE",
        "DROP",
        "ALTER",
        "EXEC",
        "UNION",
        "--",
        ";=",
        "/*",
        "*/",
        "@@"
    ];

    public static IRuleBuilderOptions<T, string> NoSqlInjection<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(text =>
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }
            return !Keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static readonly string[] HtmlTags =
    [
    "<script",
        "</script>",

        "<iframe",
        "</iframe>",

        "<object",
        "</object>",

        "<embed",
        "</embed>",

        "<form",
        "</form>",

        "<input",
        "<button",
        "</button>",

        "<textarea",
        "</textarea>",

        "<select",
        "</select>",

        "<option",
        "</option>",

        "<img",

        "<svg",
        "</svg>",

        "<video",
        "</video>",

        "<audio",
        "</audio>",

        "<link",

        "<style",
        "</style>",

        "<meta",

        "<h1",
        "<h2",
        "<h3",
        "<h4",
        "<h5",
        "<h6",

        "<div",
        "</div>",

        "<span",
        "</span>",

        "<p",
        "</p>",

        "<a",
        "</a>"
    ];

    public static IRuleBuilderOptions<T, string> NoHtml<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(text =>
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return !HtmlTags.Any(tag =>
                text.Contains(
                    tag,
                    StringComparison.OrdinalIgnoreCase));
        });
    }
}

