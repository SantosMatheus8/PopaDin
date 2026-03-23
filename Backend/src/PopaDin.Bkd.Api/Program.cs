using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.RateLimiting;
using PopaDin.Bkd.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using PopaDin.Bkd.Api.Middlewares;

namespace PopaDin.Bkd.Api;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
            c.RoutePrefix = string.Empty;
        });

        app.UseHealthChecks("/status-text");
        app.UseHealthChecks("/status-json", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var result = JsonSerializer.Serialize(new
                {
                    currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    statusApplication = report.Status.ToString()
                });
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            }
        });

        app.UseMiddleware<CustomExceptionMiddleware>();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireCors("CorsPolicy");
            endpoints.MapHealthChecks("/health");
        });
    }

    private static WebApplicationBuilder ConfigureBuild()
    {
        var builder = WebApplication.CreateBuilder();

        var env = builder.Environment.EnvironmentName;
        builder.Configuration.AddJsonFile($"appsettings.{env}.json", true, true);

        ConfigureServices(builder.Services, builder.Configuration);
        return builder;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.RegisterDatabaseDependencies(config);

        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Clear());

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<PopaDin.Bkd.Api.Validators.LoginRequestValidator>();

        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = false; });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PopaDin API",
                Version = "v1",
                Description = "API de gerenciamento financeiro pessoal"
            });
            c.DescribeAllParametersInCamelCase();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Insira o token JWT (sem o prefixo 'Bearer')",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var jwtIssuer = config["JwtSettings:Issuer"] ?? "PopaDin.Api";
        var jwtAudience = config["JwtSettings:Audience"] ?? "PopaDin.Client";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["AppSettings:Secret"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });

        services.RegisterDependencies(config);

        var allowedOrigins = config.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });
        });

        services.AddHealthChecks();
    }

    public static void Main(string[] args)
    {
        var builder = ConfigureBuild();
        var app = builder.Build();

        Configure(app, app.Environment);

        app.Run();
    }
}
