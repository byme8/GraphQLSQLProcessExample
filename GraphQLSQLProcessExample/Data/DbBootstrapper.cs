using GraphQLSQLProcessExample.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSQLProcessExample.Data;

public class DbBootstrapper
{
    public static async Task Init(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();
        
        if (await context.Processes.AnyAsync())
        {
            return;
        }
        
        var processes = Enumerable.Range(1, 100)
            .Select(i => new ProcessEntity { Name = $"P-{Guid.NewGuid()} {i}" })
            .ToArray();
        
        await context.Processes.AddRangeAsync(processes);

        foreach (var process in processes)
        {
            var extensions = Enumerable.Range(1, 10)
                .Select(i => new ExtensionEntity { Name = $"E-{Guid.NewGuid()} {i}", Process = process })
                .ToArray();
            
            await context.Extensions.AddRangeAsync(extensions);
        }
        
        await context.SaveChangesAsync();
    }
}