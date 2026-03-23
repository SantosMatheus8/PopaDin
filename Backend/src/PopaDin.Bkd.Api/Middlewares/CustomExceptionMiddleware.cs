using System.Net.Mime;
using System.Text.Json;
using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Api.Middlewares;

public class CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (PopaBaseException ex)
        {
            logger.LogWarning("Exceção de negócio: {Message}", ex.Message);
            await WriteResponseAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado");
            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.");
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message, Dictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = JsonSerializer.Serialize(new
        {
            statusCode,
            message,
            errors
        }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        await context.Response.WriteAsync(response);
    }
}
