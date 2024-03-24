using GraphQLSQLProcessExample.Data;
using GraphQLSQLProcessExample.Services.Extensions;
using GraphQLSQLProcessExample.Services.Process;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddGraphQLServer()
    .AddGraphQLSQLProcessExampleTypes();

var connectionString = configuration.GetValue<string>("DB_CONNECTION")!;
services.AddSingleton(o => new DBConfig(connectionString));
services.AddScoped<DapperContext>();

services.AddDbContextPool<AppDbContext>(options => { options.UseSqlServer(connectionString); });

services.AddScoped<ProcessService>();
services.AddScoped<ExtensionService>();

var app = builder.Build();

app.MapGraphQL();

await DbBootstrapper.Init(app.Services);

await app.RunAsync();