using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609060003L)]
public sealed class CreateInvoicesTable : Migration
{
    public const string InvoiceIdColumnName = "InvoiceId";
    public const string TaxIdColumnName = "TaxId";
    public const string ClientShortNameColumnName = "ClientShortName";
    public const string ElectronicInvoiceColumnName = "ElectronicInvoice";
    public const string CashRegisterColumnName = "CashRegister";
    public const string ThreeCashRegisterColumnName = "ThreeCashRegister";
    public const string TwoPartInvoiceColumnName = "TwoPartInvoice";
    public const string TwoPartInvoiceCopyColumnName = "TwoPartInvoiceCopy";
    public const string ThreePartInvoiceColumnName = "ThreePartInvoice";
    public const string ThreePartInvoiceCopyColumnName = "ThreePartInvoiceCopy";

    public override void Up()
    {
        Create.Table("Invoices")
            .InSchema("dbo")
            .WithColumn(InvoiceIdColumnName).AsInt32().Identity().NotNullable().PrimaryKey("PK_Invoices")
            .WithColumn(TaxIdColumnName).AsAnsiString(8).NotNullable()
            .WithColumn(ClientShortNameColumnName).AsString(50).NotNullable()
            .WithColumn(ElectronicInvoiceColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(CashRegisterColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(ThreeCashRegisterColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(TwoPartInvoiceColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(TwoPartInvoiceCopyColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(ThreePartInvoiceColumnName).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(ThreePartInvoiceCopyColumnName).AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Index("UX_Invoices_TaxId")
            .OnTable("Invoices")
            .InSchema("dbo")
            .WithOptions()
            .Unique()
            .OnColumn(TaxIdColumnName).Ascending();

        Execute.Sql("""
            ALTER TABLE [dbo].[Invoices]
            ADD CONSTRAINT [CK_Invoices_TaxId]
                CHECK (
                    LEN([TaxId]) = 8
                    AND [TaxId] COLLATE Latin1_General_100_BIN2 LIKE
                        '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                );

            ALTER TABLE [dbo].[Invoices]
            ADD CONSTRAINT [CK_Invoices_ClientShortName]
                CHECK (LEN(LTRIM(RTRIM([ClientShortName]))) > 0);
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Invoices]', N'U') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_ClientShortName];
                ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_TaxId];
            END;
            """);

        Delete.Index("UX_Invoices_TaxId")
            .OnTable("Invoices")
            .InSchema("dbo");

        Delete.Table("Invoices").InSchema("dbo");
    }
}
