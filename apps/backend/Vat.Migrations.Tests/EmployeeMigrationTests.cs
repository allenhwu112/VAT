using System.Reflection;
using FluentMigrator;
using Vat.Migrations;
using Xunit;

namespace Vat.Migrations.Tests;

public sealed class EmployeeMigrationTests
{
    [Fact]
    public void Employee_migrations_are_registered_after_the_baseline()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var tableMigration = assembly.GetType("Vat.Migrations.CreateEmployeesTable");
        var procedureMigration = assembly.GetType("Vat.Migrations.CreateEmployeeProcedures");

        Assert.NotNull(tableMigration);
        Assert.NotNull(procedureMigration);
        Assert.Equal(202608220002L, tableMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(202608220003L, procedureMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
    }

    [Fact]
    public void Employee_migrations_override_both_up_and_down()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var migrationTypes = new[]
        {
            assembly.GetType("Vat.Migrations.CreateEmployeesTable"),
            assembly.GetType("Vat.Migrations.CreateEmployeeProcedures"),
        };

        Assert.All(migrationTypes, migrationType =>
        {
            Assert.NotNull(migrationType);
            Assert.Equal(migrationType, migrationType!.GetMethod(nameof(Migration.Up))!.DeclaringType);
            Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
        });
    }

    [Fact]
    public void Employee_procedure_migration_declares_the_two_expected_procedures()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateEmployeeProcedures");

        Assert.NotNull(migrationType);
        Assert.Equal(
            "Employee_Query",
            migrationType!.GetField("QueryProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));
        Assert.Equal(
            "Employee_Command",
            migrationType.GetField("CommandProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));
    }
}
