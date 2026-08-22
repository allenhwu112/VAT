using Vat.Migrations;
using FluentMigrator;
using Xunit;

namespace Vat.Migrations.Tests;

public sealed class RunnerArgumentsTests
{
    [Fact]
    public void Parse_without_arguments_runs_migrations_up()
    {
        var options = RunnerArguments.Parse([]);

        Assert.Equal(MigrationCommand.Up, options.Command);
    }

    [Fact]
    public void Parse_check_selects_read_only_connection_check()
    {
        var options = RunnerArguments.Parse(["--check"]);

        Assert.Equal(MigrationCommand.Check, options.Command);
    }

    [Fact]
    public void Parse_down_selects_one_step_rollback()
    {
        var options = RunnerArguments.Parse(["--down"]);

        Assert.Equal(MigrationCommand.Down, options.Command);
    }

    [Fact]
    public void Parse_unknown_argument_throws_an_argument_exception()
    {
        var exception = Assert.Throws<ArgumentException>(() => RunnerArguments.Parse(["--unknown"]));

        Assert.Contains("Unknown argument", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Baseline_migration_has_the_initial_schema_version()
    {
        var migrationAttribute = Assert.Single(
            typeof(BaselineVatSchema).GetCustomAttributes(typeof(MigrationAttribute), inherit: false));

        Assert.Equal(202608220001L, ((MigrationAttribute)migrationAttribute).Version);
    }
}
