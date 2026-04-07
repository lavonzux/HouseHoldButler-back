using BackendApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Extensions;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Data);

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => controller.NotFound(result.ErrorMessage),
            ServiceErrorType.Conflict => controller.Conflict(result.ErrorMessage),
            ServiceErrorType.ValidationError => controller.BadRequest(result.ErrorMessage),
            _ => controller.StatusCode(500, result.ErrorMessage)
        };
    }
}
