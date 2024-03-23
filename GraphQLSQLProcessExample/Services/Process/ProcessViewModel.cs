namespace GraphQLSQLProcessExample.Services.Process;

public record ProcessViewModel(Guid Id, string Name);

public enum ProcessOrderByField
{
    Id,
    Name,
    CreatedAt,
}