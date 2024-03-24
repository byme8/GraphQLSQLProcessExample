using Microsoft.Data.SqlClient;

namespace GraphQLSQLProcessExample.Data;

public record DBConfig(string ConnectionString);

public class DapperContext(DBConfig config)
{
    public SqlConnection Connection => new(config.ConnectionString);
}