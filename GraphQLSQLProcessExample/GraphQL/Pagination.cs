namespace GraphQLSQLProcessExample.GraphQL;

public record Pagination<TFields>(int Page, int Size, OrderDirection OrderDirection, TFields OrderBy)
    where TFields : Enum;