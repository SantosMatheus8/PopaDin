using Dapper;
using Microsoft.Data.SqlClient;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

const int batchSize = 500;

var sqlConnectionString = args.Length > 0
    ? args[0]
    : "Server=localhost,1433;Database=master;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";

var mongoConnectionString = args.Length > 1
    ? args[1]
    : "mongodb://localhost:27017";

var mongoDatabaseName = args.Length > 2
    ? args[2]
    : "popadin";

Console.WriteLine("=== Migração Record: SQL Server → MongoDB ===");
Console.WriteLine($"SQL Server: {sqlConnectionString}");
Console.WriteLine($"MongoDB: {mongoConnectionString}/{mongoDatabaseName}");
Console.WriteLine();

try
{
    // 1. Conectar ao SQL Server
    Console.WriteLine("[1/5] Conectando ao SQL Server...");
    using var sqlConnection = new SqlConnection(sqlConnectionString);
    await sqlConnection.OpenAsync();
    Console.WriteLine("      Conectado ao SQL Server.");

    // 2. Ler todos os Records com Tags via JOIN
    Console.WriteLine("[2/5] Lendo Records com Tags do SQL Server...");

    const string query = @"
        SELECT
            r.Id AS RecordId,
            r.Operation,
            r.Value,
            r.Frequency,
            r.UserId,
            r.CreatedAt,
            r.UpdatedAt,
            t.Id AS TagId,
            t.Name AS TagName,
            t.TagType,
            t.Description AS TagDescription
        FROM Record r
        LEFT JOIN RecordTag rt ON rt.RecordId = r.Id
        LEFT JOIN Tag t ON t.Id = rt.TagId
        ORDER BY r.Id";

    var rows = (await sqlConnection.QueryAsync(query)).ToList();
    Console.WriteLine($"      {rows.Count} linhas retornadas (Records + Tags expandidos).");

    // 3. Agrupar por RecordId e transformar em documentos MongoDB
    Console.WriteLine("[3/5] Transformando registros em documentos MongoDB...");

    var grouped = rows.GroupBy(r => (int)r.RecordId);
    var documents = new List<RecordMigrationDocument>();

    foreach (var group in grouped)
    {
        var first = group.First();

        var tags = group
            .Where(r => r.TagId != null)
            .Select(r => new TagSubDoc
            {
                OriginalTagId = (int)r.TagId,
                Name = (string)r.TagName,
                TagType = r.TagType != null ? (int?)r.TagType : null,
                Description = r.TagDescription as string
            })
            .ToList();

        documents.Add(new RecordMigrationDocument
        {
            UserId = (int)first.UserId,
            Operation = (int)first.Operation,
            Value = (decimal)first.Value,
            Frequency = (int)first.Frequency,
            Tags = tags,
            CreatedAt = (DateTime)first.CreatedAt,
            UpdatedAt = (DateTime)first.UpdatedAt,
            OriginalSqlId = (int)first.RecordId
        });
    }

    Console.WriteLine($"      {documents.Count} documentos preparados.");

    // 4. Inserir em batch no MongoDB
    Console.WriteLine("[4/5] Inserindo documentos no MongoDB...");

    var mongoClient = new MongoClient(mongoConnectionString);
    var database = mongoClient.GetDatabase(mongoDatabaseName);
    var collection = database.GetCollection<RecordMigrationDocument>("records");

    var totalInserted = 0;
    for (var i = 0; i < documents.Count; i += batchSize)
    {
        var batch = documents.Skip(i).Take(batchSize).ToList();
        await collection.InsertManyAsync(batch);
        totalInserted += batch.Count;
        Console.WriteLine($"      Inseridos {totalInserted}/{documents.Count} documentos...");
    }

    // 5. Validação
    Console.WriteLine("[5/5] Validando migração...");

    var sqlCount = await sqlConnection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Record");
    var mongoCount = await collection.CountDocumentsAsync(FilterDefinition<RecordMigrationDocument>.Empty);

    Console.WriteLine($"      SQL Server: {sqlCount} records");
    Console.WriteLine($"      MongoDB:    {mongoCount} documents");

    if (sqlCount == mongoCount)
    {
        Console.WriteLine();
        Console.WriteLine("=== Migração concluída com sucesso! ===");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("=== AVISO: Contagens divergentes! Verifique os dados. ===");
    }

    // Criar índices recomendados
    Console.WriteLine();
    Console.WriteLine("Criando índices recomendados...");

    await collection.Indexes.CreateManyAsync([
        new CreateIndexModel<RecordMigrationDocument>(
            Builders<RecordMigrationDocument>.IndexKeys
                .Ascending(r => r.UserId)
                .Descending(r => r.CreatedAt)),
        new CreateIndexModel<RecordMigrationDocument>(
            Builders<RecordMigrationDocument>.IndexKeys
                .Ascending(r => r.UserId)
                .Ascending(r => r.Operation)),
        new CreateIndexModel<RecordMigrationDocument>(
            Builders<RecordMigrationDocument>.IndexKeys
                .Ascending(r => r.UserId)
                .Ascending(r => r.Frequency)),
        new CreateIndexModel<RecordMigrationDocument>(
            Builders<RecordMigrationDocument>.IndexKeys
                .Ascending("Tags.OriginalTagId"))
    ]);

    Console.WriteLine("Índices criados com sucesso!");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERRO durante a migração: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Console.ResetColor();
    Environment.Exit(1);
}

// Document classes for migration (self-contained)
public class RecordMigrationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int UserId { get; set; }
    public int Operation { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Value { get; set; }

    public int Frequency { get; set; }
    public List<TagSubDoc> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int OriginalSqlId { get; set; }
}

public class TagSubDoc
{
    public int OriginalTagId { get; set; }
    public string Name { get; set; } = "";
    public int? TagType { get; set; }
    public string? Description { get; set; }
}
