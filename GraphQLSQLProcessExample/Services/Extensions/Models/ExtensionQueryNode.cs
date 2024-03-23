namespace GraphQLSQLProcessExample.Services.Extensions.Models;

public record ExtensionQueryNode(Guid ProcessId, int Count, Extension[] Extensions);
