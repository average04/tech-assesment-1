using CustomerOnboarding.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOnboarding.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return result.ErrorType switch
        {
            ErrorType.NotFound  => controller.NotFound(BuildProblem(controller, result, StatusCodes.Status404NotFound, "Not Found")),
            ErrorType.Conflict  => controller.Conflict(BuildProblem(controller, result, StatusCodes.Status409Conflict, "Conflict")),
            ErrorType.Validation => controller.BadRequest(BuildProblem(controller, result, StatusCodes.Status400BadRequest, "Validation Failed")),
            _                   => controller.StatusCode(500, BuildProblem(controller, result, StatusCodes.Status500InternalServerError, "Unexpected Error"))
        };
    }

    private static ProblemDetails BuildProblem<T>(ControllerBase controller, Result<T> result, int status, string title) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = string.Join("; ", result.Errors),
            Instance = controller.HttpContext.Request.Path
        };
}
