using BulletinBoard.Contracts.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace BulletinBoard.Hosts.Api.Validation;

/// <summary>
/// Кастомная фабрика для преобразования ошибок FluentValidation в наш ErrorResponse.
/// </summary>
public class CustomValidationResultFactory : IFluentValidationAutoValidationResultFactory
{
    /// <summary>
    /// Создаёт IActionResult с ErrorResponse на основе ошибок валидации.
    /// </summary>
    /// <param name="context">Контекст выполнения действия контроллера.</param>
    /// <param name="validationProblemDetails">Детали ошибок валидации от FluentValidation.</param>
    /// <returns>BadRequest с ErrorResponse в формате JSON.</returns>
    public IActionResult CreateActionResult(
        ActionExecutingContext context, 
        ValidationProblemDetails? validationProblemDetails)
    {
        if (validationProblemDetails == null)
        {
            return new BadRequestResult();
        }

        var errorResponse = new ErrorResponse
        {
            Status = 400,
            Title = "Ошибка валидации",
            Detail = "Один или несколько полей содержат некорректные данные.",
            TraceId = context.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = validationProblemDetails.Errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray())
        };

        return new BadRequestObjectResult(errorResponse);
    }
}