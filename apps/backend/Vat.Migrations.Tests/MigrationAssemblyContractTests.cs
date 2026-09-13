using System.Reflection;
using FluentMigrator;
using Vat.Migrations;
using Xunit;

namespace Vat.Migrations.Tests;

public sealed class MigrationAssemblyContractTests
{
    [Fact]
    public void Assembly_contains_the_complete_ordered_vat_schema_history()
    {
        var expected = new[]
        {
            (Version: 202608220001L, TypeName: "BaselineVatSchema"),
            (Version: 202608220002L, TypeName: "CreateEmployeesTable"),
            (Version: 202608220003L, TypeName: "CreateEmployeeProcedures"),
            (Version: 202608220004L, TypeName: "CreateEmployeeContactDetails"),
            (Version: 202609060001L, TypeName: "CreateClientsTable"),
            (Version: 202609060002L, TypeName: "CreateClientProcedures"),
            (Version: 202609060003L, TypeName: "CreateInvoicesTable"),
            (Version: 202609060004L, TypeName: "CreateInvoiceProcedures"),
            (Version: 202609060005L, TypeName: "UpdateInvoiceQuantityFields"),
            (Version: 202609120001L, TypeName: "MakeClientDetailsOptional"),
            (Version: 202609120002L, TypeName: "AddClientCode"),
        };

        var actual = typeof(BaselineVatSchema).Assembly
            .GetTypes()
            .Select(type =>
            (
                Type: type,
                Attribute: type.GetCustomAttribute<MigrationAttribute>()
            ))
            .Where(item => item.Attribute is not null)
            .OrderBy(item => item.Attribute!.Version)
            .Select(item =>
            (
                Version: item.Attribute!.Version,
                TypeName: item.Type.Name
            ))
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Every_vat_schema_migration_overrides_up_and_down()
    {
        var migrationTypes = typeof(BaselineVatSchema).Assembly
            .GetTypes()
            .Where(type => type.GetCustomAttribute<MigrationAttribute>() is not null)
            .ToArray();

        Assert.Equal(11, migrationTypes.Length);
        Assert.All(migrationTypes, migrationType =>
        {
            Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Up))!.DeclaringType);
            Assert.Equal(migrationType, migrationType.GetMethod(nameof(Migration.Down))!.DeclaringType);
        });
    }
}
