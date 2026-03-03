using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Service;
using PopaDin.Bkd.Infra.Repositories;
using PopaDin.Bkd.Infra.Publishers;
using PopaDin.Application.Services;
using Azure.Messaging.ServiceBus;
using MongoDB.Driver;

namespace PopaDin.Bkd.Ioc;

[ExcludeFromCodeCoverage]
public static class ServiceModuleExtensions
{
    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(configuration);

        // MongoDB
        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            return new MongoClient(connectionString);
        });
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            return client.GetDatabase(databaseName);
        });

        // Azure Service Bus
        services.AddSingleton(sp =>
        {
            var connectionString = configuration["ServiceBusSettings:ConnectionString"];
            return new ServiceBusClient(connectionString);
        });
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var queueName = configuration["ServiceBusSettings:QueueName"];
            return client.CreateSender(queueName);
        });

        // Services & Repositories
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IRecordRepository, RecordRepository>();
        services.AddScoped<IRecordService, RecordService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAlertRepository, AlertRepository>();

        // Publishers
        services.AddScoped<IRecordEventPublisher, ServiceBusRecordEventPublisher>();
    }
}
