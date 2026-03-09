using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendApi.Models;

/// <summary>
/// Used only by EF design-time tools (dotnet ef migrations / database update).
/// Reads PostgreSQL credentials from User Secrets + environment variables so that
/// launchSettings.json is not required.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(typeof(ApplicationDbContextFactory).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            $"Server={config["POSTGRESQL:ServerUrl"]};" +
            $"User ID={config["POSTGRESQL:UserId"]};" +
            $"Password={config["POSTGRESQL:Password"]};" +
            $"Database={config["POSTGRESQL:Database"]}";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
