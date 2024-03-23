using System.Text;
using System.Text.Json;
using GraphQLSQLProcessExample.Core;
using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.GraphQL;
using GraphQLSQLProcessExample.Services.Extensions.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSQLProcessExample.Services.Extensions;

record ExtensionQueryNodeData(Guid ProcessId, int? Count, string? Json);

record ExtensionCountOnlyData(Guid ProcessId, int Count);

public class ExtensionService(AppDbContext context)
{
    public async Task<IReadOnlyDictionary<Guid, ExtensionQueryNode?>> GetExtensions(
        IReadOnlyList<Guid> processIds,
        ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page,
        SelectedFields selectedFields)
    {
//         if (selectedFields.ContainsOnly(nameof(ExtensionQueryNode.Count)))
//         {
//             var processIdsString = processIds.Select(id => $"'{id}'").Join();
//             var countSql = $"""
//                 SELECT ex1.ProcessId, COUNT(*) as Count
//                 FROM Extensions as ex1
//                 WHERE ex1.ProcessId IN ({processIdsString})
//                 GROUP BY ex1.ProcessId
//             """;
//             
//             var counts = await context.Database
//                 .SqlQueryRaw<ExtensionCountOnlyData>(countSql)
//                 .ToArrayAsync();
//             
//             return processIds.ToFullDictionary(counts
//                     .Select(data => new ExtensionQueryNode(data.ProcessId, data.Count, Array.Empty<Extension>())),
//         }
        
        var sql = new StringBuilder();
        sql.AppendLine("SELECT");

        var sqlSelect = new List<string>();
        sqlSelect.Add("ex1.ProcessId");
        sqlSelect.Add(GetCountSql(selectedFields, filter));
        sqlSelect.Add(GetExtensionsSql(selectedFields, filter, page));

        sql.AppendLine(string.Join(", ", sqlSelect));
        sql.AppendLine("FROM Extensions as ex1");
        
        var rawProcessIds = processIds.Select(id => $"'{id}'").Join();
        sql.AppendLine($"WHERE ex1.ProcessId IN ({rawProcessIds})");

        var groupBy = new List<string>();
        groupBy.Add("ex1.ProcessId");
        if (filter is not null)
        {
            groupBy.Add("ex1.Name");
        }
        var filters = GetExtensionsSqlFilters(filter, page, "ex1", $"GROUP BY {groupBy.Join()}");
        sql.AppendLine(filters);

        var rawSql = sql.ToString();
        var extensions = await context.Database
            .SqlQueryRaw<ExtensionQueryNodeData>(rawSql)
            .ToArrayAsync();

        return processIds.ToFullDictionary(extensions
                .Select(MapDataToViewModel),
            p => p.ProcessId);
    }

    private string GetExtensionsSql(SelectedFields selectedFields, ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page)
    {
        return selectedFields.Contains(nameof(ExtensionQueryNode.Extensions))
            ? $"""
               (SELECT Id, Name
                  FROM Extensions as ex2
                  WHERE ex2.ProcessId = ex1.ProcessId
                  {GetExtensionsSqlFilters(filter, page, "ex2")}
                  FOR JSON PATH) as Json
               """
            : "null as Json";
    }

    private string GetCountSql(SelectedFields selectedFields, ExtensionWhereInput? filter)
    {
        return selectedFields.Contains(nameof(ExtensionQueryNode.Count))
            ? $"""
               (SELECT COUNT(*)
                  FROM Extensions as ex2
                  WHERE ex2.ProcessId = ex1.ProcessId
                  {GetExtensionsSqlFilters(filter, null, "ex2")}) AS Count
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

    private string GetExtensionsSqlFilters(
        ExtensionWhereInput? filter,
        Pagination<ExtensionOrderByField>? page,
        string tableAlias,
        string? suffix = null)
    {
        var sql = new StringBuilder();
        if (filter is not null)
        {
            sql.AppendLine($"AND {tableAlias}.Name like '%{filter.Filter}%'");
        }

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