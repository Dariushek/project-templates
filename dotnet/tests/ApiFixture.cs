using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace RsTests;

/// <summary>
/// Base of every test class. Each test gets its own app and its own empty database.
/// Add the Given helpers here. They set state through the public API, never through the database.
/// </summary>
internal abstract class ApiFixture: IAsyncDisposable
{
    private WebApplicationFactory<Program>? host;
    private WebApplicationFactory<Program>? factory;

    protected HttpClient Client { get; private set; } = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        var connectionString = await DatabaseSetup.CreateDatabaseAsync();

        host = new WebApplicationFactory<Program>();
        factory = host.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:RsConnectionName", connectionString);
            builder.ConfigureTestServices(ReplaceServices);
        });

        await PrepareDatabaseAsync(factory.Services);
        Client = factory.CreateClient();
    }

    /// <summary>Runs after each test. The database stays: it goes away with the container.</summary>
    public async ValueTask DisposeAsync()
    {
        // Set-up may have failed half way, so each part may be missing.
        Client?.Dispose();
        if (factory is { })
        {
            await factory.DisposeAsync();
        }

        if (host is { })
        {
            await host.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>Swap clients of services that cannot run locally for fakes.</summary>
    protected virtual void ReplaceServices(IServiceCollection services)
    {
    }

    /// <summary>Create the schema, by applying the migrations when the project has them.</summary>
    protected virtual Task PrepareDatabaseAsync(IServiceProvider services)
    {
        return Task.CompletedTask;
    }
}
