using System.Text.Json;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BulletinBoard.Infrastructure.Middlewares;

/// <summary>
/// Middleware для глобальной обработки исключений.
/// Перехватывает все необработанные исключения и возвращает стандартизированный ErrorResponse.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Инициализирует middleware обработки исключений.
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает HTTP запрос и перехватывает исключения.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Обрабатывает перехваченное исключение и возвращает ErrorResponse.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorResponse) = MapExceptionToResponse(exception, context);
        
        LogException(exception, context, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Маппинг исключения в HTTP статус и ErrorResponse.
    /// </summary>
    private static (int StatusCode, ErrorResponse Response) MapExceptionToResponse(
        Exception exception,
        HttpContext context)
    {
        return exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Ресурс не найден",
                    Detail = notFound.Message,
                    TraceId = context.TraceIdentifier
                }),

            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Ошибка валидации",
                    Detail = "Один или несколько полей содержат некорректные данные.",
                    TraceId = context.TraceIdentifier,
                    Errors = validation.Errors
                }),

            ArgumentException argumentEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Некорректные параметры запроса",
                    Detail = argumentEx.Message,
                    TraceId = context.TraceIdentifier
                }),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                new ErrorResponse
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Доступ запрещен",
                    Detail = "У вас нет прав для выполнения этой операции.",
                    TraceId = context.TraceIdentifier
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Внутренняя ошибка сервера",
                    Detail = "Произошла непредвиденная ошибка. Обратитесь к администратору.",
                    TraceId = context.TraceIdentifier
                })
        };
    }

    /// <summary>
    /// Логирование исключения с контекстом запроса.
    /// </summary>
    private void LogException(Exception exception, HttpContext context, int statusCode)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["TraceId"] = context.TraceIdentifier,
                   ["RequestPath"] = context.Request.Path,
                   ["RequestMethod"] = context.Request.Method,
                   ["UserIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                   ["StatusCode"] = statusCode
               }))
        {
            _logger.Log(logLevel, exception, "Необработанное исключение: {ExceptionType}", exception.GetType().Name);
        }
    }
}