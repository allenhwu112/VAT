using System.Reflection;
using System.Linq;
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

    [Fact]
    public void Optional_client_details_migration_is_registered_after_existing_migrations()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.MakeClientDetailsOptional");

        Assert.NotNull(migrationType);
        Assert.Equal(
            202609120001L,
            migrationType!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Up))!.DeclaringType);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
    }

    [Fact]
    public void Optional_client_details_migration_allows_null_values_and_updates_command_procedure()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "MakeClientDetailsOptional.cs"));

        Assert.Contains("ALTER COLUMN [FullName] NVARCHAR(100) NULL", source);
        Assert.Contains("ALTER COLUMN [ShortName] NVARCHAR(50) NULL", source);
        Assert.Contains("ALTER COLUMN [ResponsiblePerson] NVARCHAR(100) NULL", source);
        Assert.Contains("ALTER COLUMN [Address] NVARCHAR(255) NULL", source);
        Assert.Contains("[CK_Clients_RequiredText]", source);
        Assert.Contains("[CK_Clients_OptionalText]", source);
        Assert.Contains("[FullName] IS NULL OR", source);
        Assert.Contains("CREATE OR ALTER PROCEDURE [dbo].[Client_Command]", source);
        Assert.Contains("Cannot rollback client details while NULL values exist.", source);
    }

    [Fact]
    public void Client_code_migration_is_registered_after_optional_client_details()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.AddClientCode");

        Assert.NotNull(migrationType);
        Assert.Equal(
            202609120002L,
            migrationType!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Up))!.DeclaringType);
        Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
    }

    [Fact]
    public void Client_code_migration_declares_format_and_unique_manual_code_contract()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "AddClientCode.cs"));

        Assert.Contains("ADD [ClientCode] VARCHAR(4) NULL", source);
        Assert.Contains("[CK_Clients_ClientCode]", source);
        Assert.Contains("[UX_Clients_ClientCode]", source);
        Assert.Contains("WHERE [ClientCode] IS NOT NULL", source);
        Assert.Contains("@ClientCode", source);
        Assert.Contains("[ClientCode]", source);
        Assert.Contains("Client code is invalid.", source);
    }

    [Fact]
    public void Client_code_ddl_uses_separate_batches_for_new_column_references()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "AddClientCode.cs"));

        var sqlBlocks = source
            .Split(new[] { "Execute.Sql(\"\"\"" }, StringSplitOptions.None)
            .Skip(1)
            .Select(block => block.Split(new[] { "\"\"\");" }, 2, StringSplitOptions.None)[0])
            .ToArray();

        var addColumnBlocks = sqlBlocks
            .Where(block => block.Contains("ADD [ClientCode] VARCHAR(4) NULL", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(addColumnBlocks);
        Assert.DoesNotContain("[CK_Clients_ClientCode]", addColumnBlocks[0]);
        Assert.DoesNotContain("CREATE UNIQUE INDEX [UX_Clients_ClientCode]", addColumnBlocks[0]);
        Assert.Contains(sqlBlocks, block => block.Contains("[CK_Clients_ClientCode]", StringComparison.Ordinal));
        Assert.Contains(sqlBlocks, block => block.Contains("CREATE UNIQUE INDEX [UX_Clients_ClientCode]", StringComparison.Ordinal));
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
