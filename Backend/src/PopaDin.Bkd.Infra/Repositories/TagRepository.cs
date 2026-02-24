using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Models.Tag;
using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Infra.Repositories;

public class TagRepository(SqlConnection connection, ILogger<TagRepository> logger) : ITagRepository
{
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query a ser executada: {Sql}.", TagQueries.CreateTag);
            var tagCreated = await connection.QueryAsync<Tag>(TagQueries.CreateTag, new
            {
                Name = tag.Name,
                TagType = tag.TagType,
                Description = tag.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, transaction);
            await transaction.CommitAsync();

            return tagCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao Criar Tag : {Erro}", e);
            throw;
        }
    }

    public async Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags)
    {
        var query = AddQueryPagination(listTags);
        var countQuery = AddFilters(listTags, TagQueries.Count);

        logger.LogInformation("Query a ser executada: {Sql}. with parameters: {@Parameters}", query, listTags);

        var result = await connection.QueryAsync<Tag>(
            query, new
            {
                Id = listTags.Id,
                Name = listTags.Name,
                TagType = listTags.TagType,
                Description = listTags.Description,
                Offset = (listTags.Page - 1) * listTags.ItemsPerPage,
                listTags.ItemsPerPage
            }
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, new
        {
            Id = listTags.Id,
            Name = listTags.Name,
            TagType = listTags.TagType,
            Description = listTags.Description,
            Offset = (listTags.Page - 1) * listTags.ItemsPerPage,
            listTags.ItemsPerPage
        });


        logger.LogInformation("Resultado: {@Resultado}. ", result);

        return new PaginatedResult<Tag>
        {
            Lines = result.ToList(),
            Page = listTags.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listTags.ItemsPerPage),
            TotalItens = totalLines,
            PageSize = listTags.ItemsPerPage
        };
    }

    private static string AddQueryPagination(ListTags listTags)
    {
        var query = AddFilters(listTags, TagQueries.ListTags);
        query +=
            @$"
                ORDER BY 
                {listTags.OrderBy.GetEnumDescription()} 
                {listTags.OrderDirection.GetEnumDescription()} 
                OFFSET @Offset 
                ROWS FETCH NEXT @ItemsPerPage ROWS ONLY
                ";
        return query;
    }

    private static string AddFilters(ListTags listTags, string query)
    {
        if (listTags.Id.HasValue)
            query += " AND b.Id = @Id ";
        if (listTags.TagType.HasValue)
            query += " AND b.TagType = @TagType ";
        return query;
    }

    public async Task<Tag> FindTagByIdAsync(decimal tagId)
    {
        logger.LogInformation("Query executada: {Sql}.", TagQueries.FindTagById);

        var response = await connection.QueryFirstOrDefaultAsync<Tag>(TagQueries.FindTagById,
            new { TagId = tagId });

        logger.LogInformation("Resultado: {@Resultado}. ", response);

        return response!;
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", TagQueries.UpdateTag);
            var response = await connection.ExecuteAsync(TagQueries.UpdateTag,
                new
                {
                    TagId = tag.Id,
                    Name = tag.Name,
                    TagType = tag.TagType,
                    Description = tag.Description,
                    UpdatedAt = DateTime.Now
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao editar Tag : {Erro}", e);
            throw;
        }
    }

    public async Task DeleteTagAsync(decimal tagId)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", TagQueries.DeleteTag);
            var response = await connection.ExecuteAsync(TagQueries.DeleteTag,
                new
                {
                    TagId = tagId
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao deletar Tag : {Erro}", e);
            throw;
        }
    }
}

