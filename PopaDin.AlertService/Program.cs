using Azure.Messaging.ServiceBus;
using MongoDB.Driver;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Services;
using PopaDin.AlertService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"];
    return new MongoClient(connectionString);
});
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"];
    return client.GetDatabase(databaseName);
});

// Azure Service Bus
builder.Services.AddSingleton(sp =>
{
    var rawConnectionString = builder.Configuration["ServiceBusSettings:ConnectionString"] ?? "";

    // Remove EntityPath da connection string para evitar conflito com o QueueName configurado
    var cleanedConnectionString = string.Join(";",
        rawConnectionString.Split(';')
            .Where(part => !part.Trim().StartsWith("EntityPath=", StringComparison.OrdinalIgnoreCase))
    );

    return new ServiceBusClient(cleanedConnectionString);
});
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<ServiceBusClient>();
    var queueName = builder.Configuration["ServiceBusSettings:QueueName"];
    return client.CreateProcessor(queueName, new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = 1
    });
});

// Services
builder.Services.AddScoped<IAlertRuleService, AlertRuleService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Worker
builder.Services.AddHostedService<AlertWorker>();

var host = builder.Build();
host.Run();
