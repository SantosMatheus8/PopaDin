using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Service;
using PopaDin.Bkd.Infra.Repositories;
using PopaDin.Application.Services;

namespace PopaDin.Bkd.Ioc;

[ExcludeFromCodeCoverage]
public static class ServiceModuleExtensions
{
    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(configuration);
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
    }
}