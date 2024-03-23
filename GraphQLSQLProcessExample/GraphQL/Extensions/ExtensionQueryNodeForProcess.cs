using GraphQLSQLProcessExample.Core;
using GraphQLSQLProcessExample.Services.Extensions;
using GraphQLSQLProcessExample.Services.Extensions.Models;
using GraphQLSQLProcessExample.Services.Process;
using HotChocolate.Resolvers;

namespace GraphQLSQLProcessExample.GraphQL.Extensions;

[ExtendObjectType<ProcessViewModel>]
public class ExtensionQueryNodeForProcess
{
    public async Task<ExtensionQueryNode?> GetExtensions(
        [Parent] ProcessViewModel processViewModel,
        [Service] ExtensionService service,
        ExtensionWhereInput? where,
        Pagination<ExtensionOrderByField>? page,
        IResolverContext context)
    {
        var selectedFields = context.SelectedFields();
        return await context.BatchDataLoader<Guid, ExtensionQueryNode?>((ids, _) =>
                service.GetExtensions(ids, where, page, selectedFields))
            .LoadAsync(processViewModel.Id);
    }
}
