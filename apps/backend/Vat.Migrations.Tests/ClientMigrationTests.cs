using System.Reflection;
using FluentMigrator;
using Vat.Migrations;
using Xunit;

namespace Vat.Migrations.Tests;

public sealed class ClientMigrationTests
{
    [Fact]
    public void Client_migrations_are_registered_after_employee_migrations()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var tableMigration = assembly.GetType("Vat.Migrations.CreateClientsTable");
        var procedureMigration = assembly.GetType("Vat.Migrations.CreateClientProcedures");

        Assert.NotNull(tableMigration);
        Assert.NotNull(procedureMigration);
        Assert.Equal(202609060001L, tableMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(202609060002L, procedureMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
    }

    [Fact]
    public void Client_migrations_override_both_up_and_down()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var migrationTypes = new[]
        {
            assembly.GetType("Vat.Migrations.CreateClientsTable"),
            assembly.GetType("Vat.Migrations.CreateClientProcedures"),
        };

        Assert.All(migrationTypes, migrationType =>
        {
            Assert.NotNull(migrationType);
            Assert.Equal(migrationType, migrationType!.GetMethod(nameof(Migration.Up))!.DeclaringType);
            Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
        });
    }

    [Fact]
    public void Client_table_migration_declares_the_required_columns_and_constraints()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateClientsTable");

        Assert.NotNull(migrationType);
        Assert.Equal("ClientId", migrationType!.GetField("ClientIdColumnName")!.GetValue(null));
        Assert.Equal("TaxId", migrationType.GetField("TaxIdColumnName")!.GetValue(null));
        Assert.Equal("FullName", migrationType.GetField("FullNameColumnName")!.GetValue(null));
        Assert.Equal("ShortName", migrationType.GetField("ShortNameColumnName")!.GetValue(null));
        Assert.Equal("ResponsiblePerson", migrationType.GetField("ResponsiblePersonColumnName")!.GetValue(null));
        Assert.Equal("Address", migrationType.GetField("AddressColumnName")!.GetValue(null));

        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "CreateClientsTable.cs"));

        Assert.Contains("Create.Table(\"Clients\")", source);
        Assert.Contains("WithColumn(ClientIdColumnName).AsInt32().Identity().NotNullable()", source);
        Assert.Contains("WithColumn(TaxIdColumnName).AsAnsiString(8).NotNullable()", source);
        Assert.Contains("WithColumn(FullNameColumnName).AsString(100).NotNullable()", source);
        Assert.Contains("WithColumn(ShortNameColumnName).AsString(50).NotNullable()", source);
        Assert.Contains("WithColumn(ResponsiblePersonColumnName).AsString(100).NotNullable()", source);
        Assert.Contains("WithColumn(AddressColumnName).AsString(255).NotNullable()", source);
        Assert.Contains("UX_Clients_TaxId", source);
        Assert.Contains("CK_Clients_TaxId", source);
        Assert.Contains("CK_Clients_RequiredText", source);
    }

    [Fact]
    public void Client_procedure_migration_declares_the_expected_contract()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateClientProcedures");

        Assert.NotNull(migrationType);
        Assert.Equal(
            "Client_Query",
            migrationType!.GetField("QueryProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));
        Assert.Equal(
            "Client_Command",
            migrationType.GetField("CommandProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));

        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "CreateClientProcedures.cs"));

        Assert.Contains("[dbo].[Client_Query]", source);
        Assert.Contains("[dbo].[Client_Command]", source);
        Assert.Contains("@ClientId", source);
        Assert.Contains("@TaxId", source);
        Assert.Contains("@FullName", source);
        Assert.Contains("@ShortName", source);
        Assert.Contains("@ResponsiblePerson", source);
        Assert.Contains("@Address", source);
        Assert.Contains("[ClientId]", source);
        Assert.Contains("[TaxId]", source);
        Assert.Contains("[FullName]", source);
        Assert.Contains("[ShortName]", source);
        Assert.Contains("[ResponsiblePerson]", source);
        Assert.Contains("[Address]", source);
        Assert.Contains("52007", source);
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
