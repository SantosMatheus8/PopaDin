using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Service;
using PopaDin.Bkd.Infra.Repositories;
using PopaDin.Bkd.Infra.Publishers;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using MongoDB.Driver;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Infra;
using StackExchange.Redis;

namespace PopaDin.Bkd.Ioc;

[ExcludeFromCodeCoverage]
public static class ServiceModuleExtensions
{
    public static void RegisterDatabaseDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")!;
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
    }

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

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration["RedisSettings:ConnectionString"];
            return ConnectionMultiplexer.Connect(connectionString!);
        });

        // Azure Service Bus
        services.AddSingleton(sp =>
        {
            var rawConnectionString = configuration["ServiceBusSettings:ConnectionString"] ?? "";

            var cleanedConnectionString = string.Join(";",
                rawConnectionString.Split(';')
                    .Where(part => !part.Trim().StartsWith("EntityPath=", StringComparison.OrdinalIgnoreCase))
            );

            return new ServiceBusClient(cleanedConnectionString);
        });
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var queueName = configuration["ServiceBusSettings:QueueName"];
            return client.CreateSender(queueName);
        });

        // Azure Blob Storage
        services.AddSingleton(sp =>
        {
            var connectionString = configuration["BlobStorageSettings:ConnectionString"];
            var containerName = configuration["BlobStorageSettings:ContainerName"] ?? "pdf-exports";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();
            return containerClient;
        });

        // Services
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IRecordService, RecordService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Repositories
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IRecordRepository, MongoRecordRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagCacheRepository, RedisTagCacheRepository>();
        services.AddScoped<IDashboardRepository, MongoDashboardRepository>();
        services.AddScoped<IDashboardCacheRepository, RedisDashboardCacheRepository>();
        services.AddScoped<IUserCacheRepository, RedisUserCacheRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IExportBlobRepository, BlobExportRepository>();

        // Infrastructure
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IRecordEventPublisher, ServiceBusRecordEventPublisher>();
        services.AddScoped<IExportEventPublisher, ServiceBusExportEventPublisher>();
    }
}
