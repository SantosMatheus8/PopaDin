using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using PopaDin.Bkd.Ioc;
using Microsoft.Data.SqlClient;

namespace PopaDin.Bkd.Api;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                c.RoutePrefix = string.Empty;
            });
        }
        else
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new[]
                    {
                        new OpenApiServer { Url = $"https://{httpReq.Host.Value}/PopaDin" }
                    };
                });
            });
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/PopaDin/swagger/v1/swagger.json", "v1"); });
        }

        app.UseHealthChecks("/status-text");
        app.UseHealthChecks("/status-json", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var result = JsonSerializer.Serialize(new
                {
                    currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    statusApplication = report.Status.ToString()
                });
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            }
        });

        // app.UseMiddleware<CustomExceptionMiddleware>();
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireCors();
            endpoints.MapHealthChecks("/health");
        });
    }

    private static WebApplicationBuilder ConfigureBuild(string? env)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddJsonFile($"appsettings.{env}.json", true, true);

        ConfigureServices(builder.Services, builder.Configuration);
        return builder;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<SqlConnection>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Database");
            return new SqlConnection(connectionString);
        });
        services.AddControllers();
        services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Popinha",
                Version = "v1",
                Description = "Popinha Lindo"
            });
            c.DescribeAllParametersInCamelCase();
        });
        services.RegisterDependencies(config);
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        services.AddMvc();
        services.AddHealthChecks();
    }

    private static string? GetEnvironmentVariable(string variable)
    {
        return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine)
            ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
    }

    public static void Main(string[] args)
    {
        try
        {
            var env = GetEnvironmentVariable("ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
                Console.WriteLine("VARIÁVEL DE AMBIENTE NAO ENCONTRADA");
            else
                Console.WriteLine("ENCONTROU A VARIÁVEL " + env);

            var builder = ConfigureBuild(env);
            var app = builder.Build();

            Configure(app, app.Environment);

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("erro inesperado: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
}