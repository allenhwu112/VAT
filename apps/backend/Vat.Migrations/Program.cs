using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vat.Migrations;

internal static class Program
{
    private const string ExpectedDatabaseName = "VAT";

    public static async Task<int> Main(string[] args)
    {
        RunnerArguments options;

        try
        {
            options = RunnerArguments.Parse(args);
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }

        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
            });

            var connectionString = builder.Configuration.GetConnectionString("VAT");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine("Missing configuration: ConnectionStrings:VAT.");
                return 2;
            }

            if (options.Command == MigrationCommand.Check)
            {
                return await CheckConnectionAsync(connectionString);
            }

            builder.Services
                .AddFluentMigratorCore()
                .ConfigureRunner(runnerBuilder => runnerBuilder
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(BaselineVatSchema).Assembly).For.Migrations())
                .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());

            using var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);
            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            if (options.Command == MigrationCommand.Up)
            {
                runner.MigrateUp();
                Console.WriteLine("VAT migrations applied successfully.");
            }
            else
            {
                runner.Rollback(1);
                Console.WriteLine("The latest VAT migration was rolled back successfully.");
            }

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"VAT migration command failed: {exception}");
            return 1;
        }
    }

    private static async Task<int> CheckConnectionAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT DB_NAME();";
        var databaseName = (await command.ExecuteScalarAsync())?.ToString();

        if (!string.Equals(databaseName, ExpectedDatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The connection opened database '{databaseName}', but '{ExpectedDatabaseName}' was expected.");
        }

        Console.WriteLine($"Connection OK: {connection.DataSource} / {databaseName}");
        return 0;
    }
}
