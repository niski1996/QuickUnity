using Microsoft.EntityFrameworkCore;
using QuickUnity.Data;

namespace QuickUnity;

public static class MigrateExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext  = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

    }
}