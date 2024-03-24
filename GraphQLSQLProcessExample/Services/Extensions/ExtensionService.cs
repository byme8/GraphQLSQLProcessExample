using System.Text;
using Dapper;
using GraphQLSQLProcessExample.Core;
using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.GraphQL;
using GraphQLSQLProcessExample.Services.Extensions.Models;

namespace GraphQLSQLProcessExample.Services.Extensions;

public class ExtensionService(DapperContext context)
{
    public async Task<IReadOnlyDictionary<Guid, ExtensionQueryNode?>> GetExtensions(
        IReadOnlyList<Guid> processIds,
        ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page,
        SelectedFields<ExtensionQueryNode> selectedFields)
    {
        if (processIds.Count == 0)
        {
            return processIds.ToFullDictionary(Array.Empty<ExtensionQueryNode?>(), p => p.ProcessId);
        }

        var sql = $"""
                   {GetCountSql(selectedFields, filter)}
                   {GetExtensionsSql(selectedFields, filter, page)}
                   """;

        var dataset = await context.Connection
            .QueryMultipleAsync(sql, new { ProcessIds = processIds });

        var results = Dictionary.Fill<Guid, ExtensionQueryNode?>(processIds);
        if (selectedFields.Contains(o => o.Count))
        {
            var countData = await dataset.ReadAsync<(Guid ProcessId, int Count)>();
            var counts = countData.ToDictionary(o => o.ProcessId, o => o.Count);

            foreach (var count in counts)
            {
                results[count.Key] = new ExtensionQueryNode
                {
                    ProcessId = count.Key,
                    Count = count.Value
                };
            }
        }

        if (selectedFields.Contains(o => o.Extensions))
        {
            var extensionsData = await dataset.ReadAsync<(Guid ProcessId, Guid Id, string Name)>();
            var extensions = extensionsData
                .GroupBy(o => o.ProcessId)
                .ToDictionary(
                    o => o.Key,
                    o => o
                        .Select(e => new Extension(e.Name))
                        .ToArray());

            foreach (var extension in extensions)
            {
                var node = results.GetValueOrDefault(extension.Key);
                if (node is not null)
                {
                    node.Extensions = extension.Value;
                }

                if (node is null)
                {
                    results[extension.Key] = new ExtensionQueryNode
                    {
                        ProcessId = extension.Key,
                        Extensions = extension.Value
                    };
                }
            }
        }

        return results;
    }

    private string GetExtensionsSql(
        SelectedFields<ExtensionQueryNode> selectedFields,
        ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page)
    {
        return selectedFields.Contains(o => o.Extensions)
            ? $"""
               select ProcessId,
                      Id,
                      Name
               from Extensions
               where ProcessId in @ProcessIds
                  {GetExtensionsSqlFilter(filter)}
                  {GetPaginationQuery(page)}
               """
            : string.Empty;
    }

    private string GetCountSql(SelectedFields<ExtensionQueryNode> selectedFields, ExtensionWhereInput? filter)
    {
        return selectedFields.Contains(o => o.Count)
            ? $"""
               select ProcessId, count(*)
               from Extensions
               where ProcessId in @ProcessIds
               {GetExtensionsSqlFilter(filter)}
               group by ProcessId;
               """
            : string.Empty;
    }

    private string GetExtensionsSqlFilter(
        ExtensionWhereInput? filter)
    {
        if (filter is not null)
        {
            return $"AND Name like '%{filter.Filter}%'";
        }

        return string.Empty;
    }

    public string GetPaginationQuery(
        Pagination<ExtensionOrderByField>? page,
        string? suffix = null)
    {
        var sql = new StringBuilder();
        if (page is not null)
        {
            if (suffix is not null)
            {
                sql.AppendLine(suffix);
            }

            sql.AppendLine($"ORDER BY {page.OrderBy} {page.OrderDirection}");
            sql.AppendLine($"OFFSET {page.Page * page.Size} ROWS FETCH NEXT {page.Size} ROWS ONLY");
        }
        else
        {
            if (suffix is not null)
            {
                sql.AppendLine(suffix);
            }
        }


        return sql.ToString();
    }
}