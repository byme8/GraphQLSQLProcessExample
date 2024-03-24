using System.Text;
using System.Text.Json;
using GraphQLSQLProcessExample.Core;
using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.GraphQL;
using GraphQLSQLProcessExample.Services.Extensions.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSQLProcessExample.Services.Extensions;

record ExtensionQueryNodeData(Guid ProcessId, int? Count, string? Json);

public class ExtensionService(AppDbContext context)
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
        
        var sql = new StringBuilder();
        sql.AppendLine("SELECT");

        var sqlSelect = new List<string>();
        sqlSelect.Add("ex1.ProcessId");
        sqlSelect.Add(GetCountSql(selectedFields, filter));
        sqlSelect.Add(GetExtensionsSql(selectedFields, filter, page));

        sql.AppendLine(sqlSelect.JoinLines());
        sql.AppendLine("FROM Extensions as ex1");

        var rawProcessIds = processIds.Select(id => $"'{id}'").Join();
        sql.AppendLine($"WHERE ex1.ProcessId IN ({rawProcessIds})");

        var groupBy = new List<string>();
        groupBy.Add("ex1.ProcessId");
        if (filter is not null)
        {
            groupBy.Add("ex1.Name");
        }

        sql.AppendLine(GetExtensionsSqlFilter(filter, "ex1"));
        sql.AppendLine(GetPaginationQuery(page, $"GROUP BY {groupBy.Join()}"));

        var rawSql = sql.ToString();
        var extensions = await context.Database
            .SqlQueryRaw<ExtensionQueryNodeData>(rawSql)
            .ToArrayAsync();

        // the ToFullDictionary is required to return a dictionary with all the keys
        // As a result, the GraphQL resolver will not fail and return null for the keys with missing values
        return processIds.ToFullDictionary(extensions
                .Select(MapDataToViewModel),
            p => p.ProcessId);
    }

    private string GetExtensionsSql(
        SelectedFields<ExtensionQueryNode> selectedFields,
        ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page)
    {
        return selectedFields.Contains(o => o.Extensions)
            ? $"""
               (SELECT Id, Name
                  FROM Extensions as ex2
                  WHERE ex2.ProcessId = ex1.ProcessId
                  {GetExtensionsSqlFilter(filter, "ex2")}
                  {GetPaginationQuery(page)}
                  FOR JSON PATH) as Json
               """
            : "null as Json";
    }

    private string GetCountSql(SelectedFields<ExtensionQueryNode> selectedFields, ExtensionWhereInput? filter)
    {
        return selectedFields.Contains(o => o.Count)
            ? $"""
               (SELECT COUNT(*)
                  FROM Extensions as ex2
                  WHERE ex2.ProcessId = ex1.ProcessId
                  {GetExtensionsSqlFilter(filter, "ex2")}) AS Count
               """
            : "null as Count";
    }

    private ExtensionQueryNode MapDataToViewModel(ExtensionQueryNodeData data)
    {
        var processId = data.ProcessId;
        var count = data.Count;

        var extensions = data.Json is not null
            ? JsonSerializer.Deserialize<Extension[]>(data.Json)!
            : Array.Empty<Extension>();

        return new ExtensionQueryNode(processId, count ?? 0, extensions);
    }

    private string GetExtensionsSqlFilter(
        ExtensionWhereInput? filter,
        string tableAlias)
    {
        if (filter is not null)
        {
            return $"AND {tableAlias}.Name like '%{filter.Filter}%'";
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