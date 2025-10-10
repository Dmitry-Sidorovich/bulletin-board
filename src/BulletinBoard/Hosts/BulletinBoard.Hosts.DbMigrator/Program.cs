using BulletinBoard.Hosts.DbMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddMigrationServices(builder.Configuration);
    builder.Services.AddLogging(x => x.AddSimpleConsole());

    using var app = builder.Build();
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<MigrationDbContext>();

    var pending = await db.Database.GetPendingMigrationsAsync();
    Console.WriteLine(pending.Any()
        ? $"Found {pending.Count()} pending migration(s)."
        : "No pending migrations. Database is up to date.");

    await db.Database.MigrateAsync();
    Console.WriteLine("✅ Migrations applied successfully!");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("❌ Migration failed:");
    Console.Error.WriteLine(ex.ToString());
    return 1;
}