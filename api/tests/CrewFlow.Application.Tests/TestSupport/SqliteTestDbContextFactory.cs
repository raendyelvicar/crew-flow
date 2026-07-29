using CrewFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Tests.TestSupport;

// Backs tests with a real relational engine (foreign keys, transactions) rather than
// the EF Core InMemory provider, since BookingService relies on both.
public sealed class SqliteTestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
