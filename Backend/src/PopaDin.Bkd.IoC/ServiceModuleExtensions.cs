// using Dapper;
// using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Ms.Bkd.Erp.Comercial.IoC;

[ExcludeFromCodeCoverage]
public static class ServiceModuleExtensions
{
    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(configuration);
        // services.AddScoped<IService, Service>();
    }
}