using System.Reflection;
using FluentMigrator;
using Vat.Migrations;
using Xunit;

namespace Vat.Migrations.Tests;

public sealed class InvoiceMigrationTests
{
    [Fact]
    public void Invoice_migrations_are_registered_after_client_migrations()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var tableMigration = assembly.GetType("Vat.Migrations.CreateInvoicesTable");
        var procedureMigration = assembly.GetType("Vat.Migrations.CreateInvoiceProcedures");
        var quantityMigration = assembly.GetType("Vat.Migrations.UpdateInvoiceQuantityFields");

        Assert.NotNull(tableMigration);
        Assert.NotNull(procedureMigration);
        Assert.NotNull(quantityMigration);
        Assert.Equal(202609060003L, tableMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(202609060004L, procedureMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
        Assert.Equal(202609060005L, quantityMigration!.GetCustomAttribute<MigrationAttribute>()!.Version);
    }

    [Fact]
    public void Invoice_migrations_override_both_up_and_down()
    {
        var assembly = typeof(BaselineVatSchema).Assembly;
        var migrationTypes = new[]
        {
            assembly.GetType("Vat.Migrations.CreateInvoicesTable"),
            assembly.GetType("Vat.Migrations.CreateInvoiceProcedures"),
            assembly.GetType("Vat.Migrations.UpdateInvoiceQuantityFields"),
        };

        Assert.All(migrationTypes, migrationType =>
        {
            Assert.NotNull(migrationType);
            Assert.Equal(migrationType, migrationType!.GetMethod(nameof(Migration.Up))!.DeclaringType);
            Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
        });
    }

    [Fact]
    public void Invoice_table_migration_declares_the_required_columns_and_constraints()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateInvoicesTable");

        Assert.NotNull(migrationType);
        var expectedColumns = new Dictionary<string, string>
        {
            ["InvoiceIdColumnName"] = "InvoiceId",
            ["TaxIdColumnName"] = "TaxId",
            ["ClientShortNameColumnName"] = "ClientShortName",
            ["ElectronicInvoiceColumnName"] = "ElectronicInvoice",
            ["CashRegisterColumnName"] = "CashRegister",
            ["ThreeCashRegisterColumnName"] = "ThreeCashRegister",
            ["TwoPartInvoiceColumnName"] = "TwoPartInvoice",
            ["TwoPartInvoiceCopyColumnName"] = "TwoPartInvoiceCopy",
            ["ThreePartInvoiceColumnName"] = "ThreePartInvoice",
            ["ThreePartInvoiceCopyColumnName"] = "ThreePartInvoiceCopy",
        };

        foreach (var (fieldName, expectedValue) in expectedColumns)
        {
            Assert.Equal(expectedValue, migrationType!.GetField(fieldName)!.GetValue(null));
        }

        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "CreateInvoicesTable.cs"));

        Assert.Contains("Create.Table(\"Invoices\")", source);
        Assert.Contains("WithColumn(InvoiceIdColumnName).AsInt32().Identity().NotNullable()", source);
        Assert.Contains("WithColumn(TaxIdColumnName).AsAnsiString(8).NotNullable()", source);
        Assert.Contains("WithColumn(ClientShortNameColumnName).AsString(50).NotNullable()", source);
        Assert.Contains("AsBoolean().NotNullable().WithDefaultValue(false)", source);
        Assert.Contains("UX_Invoices_TaxId", source);
        Assert.Contains("CK_Invoices_TaxId", source);
        Assert.Contains("CK_Invoices_ClientShortName", source);
    }

    [Fact]
    public void Invoice_procedure_migration_declares_the_expected_contract()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.CreateInvoiceProcedures");

        Assert.NotNull(migrationType);
        Assert.Equal(
            "Invoice_Query",
            migrationType!.GetField("QueryProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));
        Assert.Equal(
            "Invoice_Command",
            migrationType.GetField("CommandProcedureName", BindingFlags.Public | BindingFlags.Static)!.GetValue(null));

        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "CreateInvoiceProcedures.cs"));

        Assert.Contains("[dbo].[Invoice_Query]", source);
        Assert.Contains("[dbo].[Invoice_Command]", source);
        Assert.Contains("@InvoiceId", source);
        Assert.Contains("@TaxId", source);
        Assert.Contains("@ClientShortName", source);
        Assert.Contains("@ElectronicInvoice", source);
        Assert.Contains("@CashRegister", source);
        Assert.Contains("@ThreeCashRegister", source);
        Assert.Contains("@TwoPartInvoice", source);
        Assert.Contains("@TwoPartInvoiceCopy", source);
        Assert.Contains("@ThreePartInvoice", source);
        Assert.Contains("@ThreePartInvoiceCopy", source);
        Assert.Contains("[InvoiceId]", source);
        Assert.Contains("[TaxId]", source);
        Assert.Contains("[ClientShortName]", source);
        Assert.Contains("[ElectronicInvoice]", source);
        Assert.Contains("[ThreePartInvoiceCopy]", source);
        Assert.Contains("53007", source);
    }

    [Fact]
    public void Invoice_quantity_migration_declares_integer_ranges_and_safe_rollback()
    {
        var migrationType = typeof(BaselineVatSchema).Assembly
            .GetType("Vat.Migrations.UpdateInvoiceQuantityFields");

        Assert.NotNull(migrationType);
        var source = File.ReadAllText(FindRepositoryFile(
            "apps",
            "backend",
            "Vat.Migrations",
            "Migrations",
            "UpdateInvoiceQuantityFields.cs"));

        Assert.Contains("ALTER COLUMN [CashRegister] INT NOT NULL", source);
        Assert.Contains("ALTER COLUMN [ThreeCashRegister] INT NOT NULL", source);
        Assert.Contains("ALTER COLUMN [TwoPartInvoice] INT NOT NULL", source);
        Assert.Contains("ALTER COLUMN [TwoPartInvoiceCopy] INT NOT NULL", source);
        Assert.Contains("ALTER COLUMN [ThreePartInvoice] INT NOT NULL", source);
        Assert.Contains("ALTER COLUMN [ThreePartInvoiceCopy] INT NOT NULL", source);
        Assert.Contains("BETWEEN 0 AND 99", source);
        Assert.Contains("CREATE OR ALTER PROCEDURE [dbo].[Invoice_Query]", source);
        Assert.Contains("@CashRegister INT = NULL", source);
        Assert.Contains("@ThreePartInvoiceCopy INT = NULL", source);
        Assert.Contains("NOT IN (0, 1)", source);
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
