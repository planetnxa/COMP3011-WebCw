using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Tests.ApiTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";
    private SqliteConnection _keepAliveConnection = null!;

    private string ConnectionString => $"Data Source={_dbName};Mode=Memory;Cache=Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:FragranceDb", ConnectionString);
    }

    public async Task InitializeAsync()
    {
        // Open a sentinel connection to keep the shared in-memory DB alive
        _keepAliveConnection = new SqliteConnection(ConnectionString);
        await _keepAliveConnection.OpenAsync();

        // Create the tables that the controllers expect
        using var cmd = _keepAliveConnection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS perfumeData (
                perfumeId INTEGER PRIMARY KEY AUTOINCREMENT,
                url TEXT DEFAULT '',
                Perfume TEXT DEFAULT '',
                Brand TEXT DEFAULT '',
                Country TEXT DEFAULT '',
                Gender TEXT DEFAULT '',
                Rating REAL DEFAULT 0,
                Year TEXT DEFAULT '',
                Accords TEXT DEFAULT '',
                Perfumers TEXT DEFAULT ''
            );
            CREATE TABLE IF NOT EXISTS perfumeNotes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Note TEXT DEFAULT '',
                Type TEXT DEFAULT '',
                PerfumeId INTEGER DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS userProfiles (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT DEFAULT '',
                password TEXT DEFAULT '',
                name TEXT DEFAULT '',
                firstLogin INTEGER DEFAULT 0
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_keepAliveConnection != null)
        {
            await _keepAliveConnection.CloseAsync();
            await _keepAliveConnection.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
