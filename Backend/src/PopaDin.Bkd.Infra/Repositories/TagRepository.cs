using Dapper;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Infra.Repositories;

public class TagRepository(IDbConnectionFactory connectionFactory, ILogger<TagRepository> logger) : ITagRepository
{
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Criando Tag no banco de dados");
            var tagCreated = await connection.QueryAsync<Tag>(TagQueries.CreateTag, new
            {
                Name = tag.Name,
                TagType = tag.TagType,
                Description = tag.Description,
                UserId = tag.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, transaction);
            transaction.Commit();

            return tagCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao Criar Tag: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags)
    {
        var query = AddQueryPagination(listTags);
        var countQuery = AddFilters(listTags, TagQueries.Count);

        logger.LogInformation("Listando Tags com paginação");

        var parameters = new
        {
            Id = listTags.Id,
            Name = listTags.Name,
            TagType = listTags.TagType,
            Description = listTags.Description,
            UserId = listTags.UserId,
            Offset = (listTags.Page - 1) * listTags.ItemsPerPage,
            listTags.ItemsPerPage
        };

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Tag, User, Tag>(
            query,
            (tag, user) =>
            {
                tag.User = user;
                return tag;
            },
            parameters,
            splitOn: "UserId"
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, parameters);

        return new PaginatedResult<Tag>
        {
            Lines = result.ToList(),
            Page = listTags.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listTags.ItemsPerPage),
            TotalItems = totalLines,
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
        query += " AND t.UserId = @UserId ";
        if (listTags.Id.HasValue)
            query += " AND t.Id = @Id ";
        if (listTags.TagType.HasValue)
            query += " AND t.TagType = @TagType ";
        if (!string.IsNullOrWhiteSpace(listTags.Name))
            query += " AND t.Name LIKE '%' + @Name + '%' ";
        if (!string.IsNullOrWhiteSpace(listTags.Description))
            query += " AND t.Description LIKE '%' + @Description + '%' ";
        return query;
    }

    public async Task<List<Tag>> FindTagsByIdsAsync(List<int> ids, int userId)
    {
        logger.LogInformation("Buscando Tags por Ids");

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Tag>(TagQueries.FindTagsByIds, new { Ids = ids, UserId = userId });

        return result.ToList();
    }

    public async Task<Tag> FindTagByIdAsync(int tagId, int userId)
    {
        logger.LogInformation("Buscando Tag: {TagId}", tagId);

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Tag, User, Tag>(
            TagQueries.FindTagById,
            (tag, user) =>
            {
                tag.User = user;
                return tag;
            },
            new { TagId = tagId, UserId = userId },
            splitOn: "UserId"
        );

        return result.FirstOrDefault()!;
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando Tag: {TagId}", tag.Id);
            await connection.ExecuteAsync(TagQueries.UpdateTag,
                new
                {
                    TagId = tag.Id,
                    Name = tag.Name,
                    TagType = tag.TagType,
                    Description = tag.Description,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao editar Tag: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteTagAsync(int tagId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Deletando Tag: {TagId}", tagId);
            await connection.ExecuteAsync(TagQueries.DeleteTag,
                new { TagId = tagId }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao deletar Tag: {Message}", e.Message);
            throw;
        }
    }
}
