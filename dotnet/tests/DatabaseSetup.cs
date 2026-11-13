using System.Data.Common;
#if (postgres)
using Testcontainers.PostgreSql;
#endif
#if (mssql)
using Testcontainers.MsSql;
#endif

namespace RsTests;

/// <summary>One container per test run, one empty database per test.</summary>
internal static class DatabaseSetup
{
#if (postgres)
    private const string Image = "postgres:18.1-alpine";
    private static PostgreSqlContainer container = null!;
#endif
#if (mssql)
    private const string Image = "mcr.microsoft.com/mssql/server:2025-latest";
    private static MsSqlContainer container = null!;
#endif

    [Before(Assembly)]
    public static async Task StartAsync()
    {
        // Data on tmpfs: nothing outlives the run, so nothing needs the disk.
#if (postgres)
        container = new PostgreSqlBuilder(Image)
            .WithTmpfsMount("/var/lib/postgresql")
            .Build();
#endif
#if (mssql)
        container = new MsSqlBuilder(Image)
            .WithTmpfsMount("/var/opt/mssql/data")
            .WithTmpfsMount("/var/opt/mssql/log")
            .WithTmpfsMount("/var/opt/mssql/secrets")
            .Build();
#endif

        await container.StartAsync();
    }

    [After(Assembly)]
    public static async Task StopAsync()
    {
        await container.DisposeAsync();
    }

    /// <summary>Creates an empty database and returns its connection string.</summary>
    public static async Task<string> CreateDatabaseAsync()
    {
        var name = $"test_{Guid.NewGuid():N}";
#if (postgres)
        var created = await container.ExecScriptAsync($"CREATE DATABASE \"{name}\"");
#endif
#if (mssql)
        var created = await container.ExecScriptAsync($"CREATE DATABASE [{name}]");
#endif
        if (created.ExitCode != 0)
        {
            throw new InvalidOperationException($"Could not create database {name}: {created.Stderr}");
        }

        var connectionString = new DbConnectionStringBuilder
        {
            ConnectionString = container.GetConnectionString(),
            ["Database"] = name,
        };

        return connectionString.ConnectionString;
    }
}
