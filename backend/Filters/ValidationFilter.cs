using backend.GenericResponse;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Filters;

internal sealed class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errorCodes = context.ModelState
                .Values
                .SelectMany(v => v.Errors)
                .Select(e =>
                {
                    if (int.TryParse(e.ErrorMessage, out var code))
                    {
                        return code;
                    }

                    return CustomCodes.InvalidInput;
                })
                .Distinct()
                .ToList();

            context.Result =
                new BadRequestObjectResult(
                    ResponseResults<List<int>>
                    .Failure(errorCodes.First()));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
