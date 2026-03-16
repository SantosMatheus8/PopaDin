using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Api.Middlewares;
using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Api.Tests.Middlewares;

public class CustomExceptionMiddlewareTests
{
    private readonly ILogger<CustomExceptionMiddleware> _logger = Substitute.For<ILogger<CustomExceptionMiddleware>>();

    private CustomExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new CustomExceptionMiddleware(next, _logger);
    }

    private static async Task<JsonElement> GetResponseJson(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<JsonElement>(body);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        RequestDelegate next = _ => throw new NotFoundException("Recurso não encontrado");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().Be("Recurso não encontrado");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnprocessableEntityException_ShouldReturn422()
    {
        RequestDelegate next = _ => throw new UnprocessableEntityException("Dados inválidos");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(422);
        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().Be("Dados inválidos");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedException_ShouldReturn401()
    {
        RequestDelegate next = _ => throw new UnauthorizedException("Credenciais inválidas");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().Be("Credenciais inválidas");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedException_ShouldReturn500WithGenericMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Something broke");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().Be("Ocorreu um erro inesperado.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedException_ShouldNotLeakInternalDetails()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Internal secret error");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().NotContain("Internal secret error");
    }

    [Fact]
    public async Task InvokeAsync_WhenException_ShouldReturnJsonContentType()
    {
        RequestDelegate next = _ => throw new Exception("error");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_ResponseBody_ShouldBeValidJsonWithErrorMessageProperty()
    {
        RequestDelegate next = _ => throw new NotFoundException("Test message");
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var json = await GetResponseJson(context);
        json.GetProperty("ErrorMessage").GetString().Should().Be("Test message");
    }
}
