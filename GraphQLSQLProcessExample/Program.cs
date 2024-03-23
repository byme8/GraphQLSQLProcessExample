using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.Services;
using GraphQLSQLProcessExample.Services.Extensions;
using GraphQLSQLProcessExample.Services.Process;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddGraphQLServer()
    .AddGraphQLSQLProcessExampleTypes();

services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(
        @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ProcessDB;Integrated Security=SSPI");
});

services.AddScoped<ProcessService>();
services.AddScoped<ExtensionService>();

var app = builder.Build();

app.MapGraphQL();

await DbBootstrapper.Init(app.Services);

await app.RunAsync();