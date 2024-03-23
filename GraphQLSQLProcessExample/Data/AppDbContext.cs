using GraphQLSQLProcessExample.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSQLProcessExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<ProcessEntity> Processes { get; set; }

    public DbSet<ExtensionEntity> Extensions { get; set; }
}