using Azure.Messaging.ServiceBus;
using MongoDB.Driver;
using PopaDin.RecurrenceService.Interfaces;
using PopaDin.RecurrenceService.Services;
using PopaDin.RecurrenceService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// MongoDB (IMongoDatabase é thread-safe, pode ser singleton)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"];
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"];
    return client.GetDatabase(databaseName);
});

// Azure Service Bus
builder.Services.AddSingleton(sp =>
{
    var rawConnectionString = builder.Configuration["ServiceBusSettings:ConnectionString"] ?? "";

    var cleanedConnectionString = string.Join(";",
        rawConnectionString.Split(';')
            .Where(part => !part.Trim().StartsWith("EntityPath=", StringComparison.OrdinalIgnoreCase))
    );

    return new ServiceBusClient(cleanedConnectionString);
});

// Services
builder.Services.AddScoped<IRecurrenceProcessor, RecurrenceProcessor>();
builder.Services.AddScoped<INotificationPublisher, ServiceBusNotificationPublisher>();
builder.Services.AddScoped<IBalanceUpdater, SqlBalanceUpdater>();

// Index Initializer
builder.Services.AddHostedService<MongoIndexInitializer>();

// Worker
builder.Services.AddHostedService<RecurrenceWorker>();

var host = builder.Build();
host.Run();
