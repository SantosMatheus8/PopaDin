using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Infra.Documents;

namespace PopaDin.Bkd.Infra.Repositories;

public class MongoDashboardRepository(IMongoDatabase database, ILogger<MongoDashboardRepository> logger)
    : IDashboardRepository
{
    private IMongoCollection<RecordDocument> Collection =>
        database.GetCollection<RecordDocument>("records");

    public async Task<DashboardResult> GetDashboardDataAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando dados do dashboard no MongoDB para o usuário {UserId}", userId);

        var builder = Builders<RecordDocument>.Filter;

        var notRecurring = builder.Or(
            builder.Eq(r => r.Frequency, (int)FrequencyEnum.OneTime),
            builder.And(
                builder.Ne(r => r.InstallmentGroupId, (string?)null),
                builder.Exists(r => r.InstallmentGroupId, true)
            )
        );

        var matchFilter = builder.Eq(r => r.UserId, userId)
                          & builder.Gte(r => r.ReferenceDate, startDate)
                          & builder.Lte(r => r.ReferenceDate, endDate)
                          & notRecurring;

        var pipeline = Collection.Aggregate()
            .Match(matchFilter)
            .Facet(
                AggregateFacet.Create("summary",
                    PipelineDefinition<RecordDocument, BsonDocument>.Create(
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            { "totalDeposits", new BsonDocument("$sum",
                                new BsonDocument("$cond", new BsonArray
                                {
                                    new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Deposit }),
                                    "$Value",
                                    0
                                }))
                            },
                            { "totalOutflows", new BsonDocument("$sum",
                                new BsonDocument("$cond", new BsonArray
                                {
                                    new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Outflow }),
                                    "$Value",
                                    0
                                }))
                            },
                            { "recordCount", new BsonDocument("$sum", 1) }
                        })
                    )
                ),
                AggregateFacet.Create("spendingByTag",
                    PipelineDefinition<RecordDocument, BsonDocument>.Create(
                        new BsonDocument("$match", new BsonDocument("Operation", (int)OperationEnum.Outflow)),
                        new BsonDocument("$unwind", "$Tags"),
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", new BsonDocument
                                {
                                    { "tagId", "$Tags.OriginalTagId" },
                                    { "tagName", "$Tags.Name" }
                                }
                            },
                            { "totalSpent", new BsonDocument("$sum", "$Value") }
                        }),
                        new BsonDocument("$sort", new BsonDocument("totalSpent", -1))
                    )
                ),
                AggregateFacet.Create("latestRecords",
                    PipelineDefinition<RecordDocument, BsonDocument>.Create(
                        new BsonDocument("$sort", new BsonDocument("ReferenceDate", -1)),
                        new BsonDocument("$limit", 5)
                    )
                ),
                AggregateFacet.Create("topDeposits",
                    PipelineDefinition<RecordDocument, BsonDocument>.Create(
                        new BsonDocument("$match", new BsonDocument("Operation", (int)OperationEnum.Deposit)),
                        new BsonDocument("$sort", new BsonDocument("Value", -1)),
                        new BsonDocument("$limit", 5)
                    )
                ),
                AggregateFacet.Create("topOutflows",
                    PipelineDefinition<RecordDocument, BsonDocument>.Create(
                        new BsonDocument("$match", new BsonDocument("Operation", (int)OperationEnum.Outflow)),
                        new BsonDocument("$sort", new BsonDocument("Value", -1)),
                        new BsonDocument("$limit", 5)
                    )
                )
            );

        var facetResult = await pipeline.FirstOrDefaultAsync();

        var result = new DashboardResult();

        if (facetResult == null)
            return result;

        var summaryDocs = facetResult.Facets.First(f => f.Name == "summary").Output<BsonDocument>();
        if (summaryDocs.Count > 0)
        {
            var s = summaryDocs[0];
            result.Summary = new DashboardSummary
            {
                TotalDeposits = s["totalDeposits"].IsDecimal128 ? (decimal)s["totalDeposits"].AsDecimal128 : Convert.ToDecimal(s["totalDeposits"].ToDouble()),
                TotalOutflows = s["totalOutflows"].IsDecimal128 ? (decimal)s["totalOutflows"].AsDecimal128 : Convert.ToDecimal(s["totalOutflows"].ToDouble()),
                RecordCount = s["recordCount"].AsInt32
            };
            result.Summary.Balance = result.Summary.TotalDeposits - result.Summary.TotalOutflows;
        }

        var spendingDocs = facetResult.Facets.First(f => f.Name == "spendingByTag").Output<BsonDocument>();
        result.SpendingByTag = spendingDocs.Select(d => new DashboardSpendingByTag
        {
            TagId = d["_id"]["tagId"].AsInt32,
            TagName = d["_id"]["tagName"].AsString,
            TotalSpent = d["totalSpent"].IsDecimal128 ? (decimal)d["totalSpent"].AsDecimal128 : Convert.ToDecimal(d["totalSpent"].ToDouble())
        }).ToList();

        var latestDocs = facetResult.Facets.First(f => f.Name == "latestRecords").Output<BsonDocument>();
        result.LatestRecords = latestDocs.Select(MapBsonToRecord).ToList();

        var topDepositDocs = facetResult.Facets.First(f => f.Name == "topDeposits").Output<BsonDocument>();
        result.TopDeposits = topDepositDocs.Select(MapBsonToRecord).ToList();

        var topOutflowDocs = facetResult.Facets.First(f => f.Name == "topOutflows").Output<BsonDocument>();
        result.TopOutflows = topOutflowDocs.Select(MapBsonToRecord).ToList();

        return result;
    }

    public async Task<DashboardSummary> GetPeriodSummaryAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando resumo do período no MongoDB para o usuário {UserId}", userId);

        var builder = Builders<RecordDocument>.Filter;

        var notRecurring = builder.Or(
            builder.Eq(r => r.Frequency, (int)FrequencyEnum.OneTime),
            builder.And(
                builder.Ne(r => r.InstallmentGroupId, (string?)null),
                builder.Exists(r => r.InstallmentGroupId, true)
            )
        );

        var matchFilter = builder.Eq(r => r.UserId, userId)
                          & builder.Gte(r => r.ReferenceDate, startDate)
                          & builder.Lte(r => r.ReferenceDate, endDate)
                          & notRecurring;

        var pipeline = Collection.Aggregate()
            .Match(matchFilter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "totalDeposits", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Deposit }),
                        "$Value",
                        0
                    }))
                },
                { "totalOutflows", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Outflow }),
                        "$Value",
                        0
                    }))
                },
                { "recordCount", new BsonDocument("$sum", 1) }
            });

        var doc = await pipeline.FirstOrDefaultAsync();

        if (doc == null)
            return new DashboardSummary();

        var deposits = doc["totalDeposits"].IsDecimal128
            ? (decimal)doc["totalDeposits"].AsDecimal128
            : Convert.ToDecimal(doc["totalDeposits"].ToDouble());

        var outflows = doc["totalOutflows"].IsDecimal128
            ? (decimal)doc["totalOutflows"].AsDecimal128
            : Convert.ToDecimal(doc["totalOutflows"].ToDouble());

        return new DashboardSummary
        {
            TotalDeposits = deposits,
            TotalOutflows = outflows,
            RecordCount = doc["recordCount"].AsInt32,
            Balance = deposits - outflows
        };
    }

    public async Task<List<DashboardMonthlyTrend>> GetMonthlyTrendAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando tendência mensal no MongoDB para o usuário {UserId}", userId);

        var builder = Builders<RecordDocument>.Filter;

        var notRecurring = builder.Or(
            builder.Eq(r => r.Frequency, (int)FrequencyEnum.OneTime),
            builder.And(
                builder.Ne(r => r.InstallmentGroupId, (string?)null),
                builder.Exists(r => r.InstallmentGroupId, true)
            )
        );

        var matchFilter = builder.Eq(r => r.UserId, userId)
                          & builder.Gte(r => r.ReferenceDate, startDate)
                          & builder.Lte(r => r.ReferenceDate, endDate)
                          & notRecurring;

        var pipeline = Collection.Aggregate()
            .Match(matchFilter)
            .Group(new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "year", new BsonDocument("$year", "$ReferenceDate") },
                        { "month", new BsonDocument("$month", "$ReferenceDate") }
                    }
                },
                { "totalDeposits", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Deposit }),
                        "$Value",
                        0
                    }))
                },
                { "totalOutflows", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Operation", (int)OperationEnum.Outflow }),
                        "$Value",
                        0
                    }))
                }
            })
            .Sort(new BsonDocument { { "_id.year", 1 }, { "_id.month", 1 } });

        var docs = await pipeline.ToListAsync();

        return docs.Select(d =>
        {
            var deposits = d["totalDeposits"].IsDecimal128
                ? (decimal)d["totalDeposits"].AsDecimal128
                : Convert.ToDecimal(d["totalDeposits"].ToDouble());

            var outflows = d["totalOutflows"].IsDecimal128
                ? (decimal)d["totalOutflows"].AsDecimal128
                : Convert.ToDecimal(d["totalOutflows"].ToDouble());

            return new DashboardMonthlyTrend
            {
                Year = d["_id"]["year"].AsInt32,
                Month = d["_id"]["month"].AsInt32,
                TotalDeposits = deposits,
                TotalOutflows = outflows
            };
        }).ToList();
    }

    private static Record MapBsonToRecord(BsonDocument doc)
    {
        return new Record
        {
            Id = doc["_id"].AsObjectId.ToString(),
            Name = doc.Contains("Name") && !doc["Name"].IsBsonNull ? doc["Name"].AsString : "",
            Operation = (OperationEnum)doc["Operation"].AsInt32,
            Value = doc["Value"].IsDecimal128 ? (decimal)doc["Value"].AsDecimal128 : Convert.ToDecimal(doc["Value"].ToDouble()),
            Frequency = (FrequencyEnum)doc["Frequency"].AsInt32,
            Tags = doc.Contains("Tags") ? doc["Tags"].AsBsonArray.Select(t =>
            {
                var tagDoc = t.AsBsonDocument;
                return new Domain.Models.Tag
                {
                    Id = tagDoc["OriginalTagId"].AsInt32,
                    Name = tagDoc["Name"].AsString,
                    TagType = tagDoc.Contains("TagType") && !tagDoc["TagType"].IsBsonNull
                        ? (OperationEnum)tagDoc["TagType"].AsInt32
                        : null,
                    Description = tagDoc.Contains("Description") && !tagDoc["Description"].IsBsonNull
                        ? tagDoc["Description"].AsString
                        : null,
                    Color = tagDoc.Contains("Color") && !tagDoc["Color"].IsBsonNull
                        ? tagDoc["Color"].AsString
                        : null
                };
            }).ToList() : [],
            User = new User { Id = doc["UserId"].AsInt32 },
            ReferenceDate = doc.Contains("ReferenceDate") && !doc["ReferenceDate"].IsBsonNull
                ? doc["ReferenceDate"].ToUniversalTime()
                : doc["CreatedAt"].ToUniversalTime(),
            CreatedAt = doc["CreatedAt"].ToUniversalTime(),
            UpdatedAt = doc["UpdatedAt"].ToUniversalTime()
        };
    }
}
