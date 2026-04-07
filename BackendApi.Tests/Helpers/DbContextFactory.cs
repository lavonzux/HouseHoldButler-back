using BackendApi.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Tests.Helpers;

/// <summary>
/// Manages a shared SQLite in-memory connection for a single test.
/// SQLite in-memory databases only live as long as the connection is open,
/// so all DbContext instances for the same test must share one connection.
///
/// Java analogy: like sharing an H2 in-memory DataSource across multiple
/// EntityManager instances within the same test transaction.
///
/// Usage:
///   await using var db = new TestDatabase();
///   using var seedCtx = db.CreateContext();   // seed
///   using var svcCtx  = db.CreateContext();   // service under test
///   using var readCtx = db.CreateContext();   // assertions
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Create schema (tables, seed data) once on the shared connection
        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
