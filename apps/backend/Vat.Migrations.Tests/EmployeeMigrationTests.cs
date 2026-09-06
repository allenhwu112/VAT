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

    [Fact]
    public void Employee_contact_migration_is_registered_after_employee_procedures()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateEmployeeContactDetails");

        Assert.NotNull(migrationType);
        Assert.Equal(202608220004L, migrationType!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Up))!.DeclaringType);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
    }

    [Fact]
    public void Employee_contact_migration_declares_the_new_columns_and_procedure_contract()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateEmployeeContactDetails");

        Assert.NotNull(migrationType);
        Assert.Equal("ContactPhone", migrationType!.GetField("ContactPhoneColumnName")!.GetValue(null));
        Assert.Equal("Address", migrationType.GetField("AddressColumnName")!.GetValue(null));
        Assert.Equal("BirthDate", migrationType.GetField("BirthDateColumnName")!.GetValue(null));

        var sourcePath = FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "CreateEmployeeContactDetails.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("[dbo].[Employee_Query]", source);
        Assert.Contains("[dbo].[Employee_Command]", source);
        Assert.Contains("@ContactPhone", source);
        Assert.Contains("@Address", source);
        Assert.Contains("@BirthDate", source);
        Assert.Contains("[ContactPhone]", source);
        Assert.Contains("[Address]", source);
        Assert.Contains("[BirthDate]", source);
    }

    private static string FindRepositoryFile(params string[] pathSegments)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine([directory.FullName, .. pathSegments]);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Could not find the repository file.", Path.Combine(pathSegments));
    }
}
