namespace GraphQLSQLProcessExample.Services.Extensions.Models;

public class ExtensionQueryNode
{
    public Guid ProcessId { get; set; }

    public int Count { get; set; }

    public Extension[] Extensions { get; set; }
}