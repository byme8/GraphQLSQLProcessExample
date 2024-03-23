using GraphQLSQLProcessExample.Services.Process;

namespace GraphQLSQLProcessExample.GraphQL.Process;

[QueryType]
public class ProcessQueryNode
{
    public async Task<ProcessViewModel[]> GetProcesses(
        [Service] ProcessService service,
        Pagination<ProcessOrderByField> page)
    {
        return await service.GetProcesses(page);
    }
}