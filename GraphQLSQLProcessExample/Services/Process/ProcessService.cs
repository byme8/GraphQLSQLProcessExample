using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.GraphQL;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSQLProcessExample.Services.Process;

public class ProcessService(AppDbContext context)
{
    public async Task<ProcessViewModel[]> GetProcesses(Pagination<ProcessOrderByField> page)
    {
        var orderBySql = $"ORDER BY {page.OrderBy} {page.OrderDirection}";
        var limitSql = $"OFFSET {page.Page * page.Size} ROWS FETCH NEXT {page.Size} ROWS ONLY";

        var sql = $"""
                   select Id, Name from Processes
                   {orderBySql}
                   {limitSql}
                   """;
        
        var processes = await context.Database
            .SqlQueryRaw<ProcessViewModel>(sql)
            .ToArrayAsync();

        return processes;
    }
}