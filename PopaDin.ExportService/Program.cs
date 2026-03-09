using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using MongoDB.Driver;
using PopaDin.ExportService.Interfaces;
using PopaDin.ExportService.Services;
using PopaDin.ExportService.Workers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

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

// Azure Blob Storage
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["BlobStorageSettings:ConnectionString"];
    var containerName = builder.Configuration["BlobStorageSettings:ContainerName"] ?? "pdf-exports";
    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    containerClient.CreateIfNotExists();
    return containerClient;
});

// Services
builder.Services.AddScoped<IRecordQueryService, RecordQueryService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Worker
builder.Services.AddHostedService<ExportWorker>();

var host = builder.Build();
host.Run();
