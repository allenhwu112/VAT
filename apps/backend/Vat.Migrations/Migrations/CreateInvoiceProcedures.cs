using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609060004L)]
public sealed class CreateInvoiceProcedures : Migration
{
    public const string QueryProcedureName = "Invoice_Query";
    public const string CommandProcedureName = "Invoice_Command";

    public override void Up()
    {
        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Invoice_Query]
                @InvoiceId INT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [InvoiceId],
                    [TaxId],
                    [ClientShortName],
                    [ElectronicInvoice],
                    [CashRegister],
                    [ThreeCashRegister],
                    [TwoPartInvoice],
                    [TwoPartInvoiceCopy],
                    [ThreePartInvoice],
                    [ThreePartInvoiceCopy]
                FROM [dbo].[Invoices]
                WHERE @InvoiceId IS NULL OR [InvoiceId] = @InvoiceId
                ORDER BY [InvoiceId] DESC;
            END;
            """);

        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Invoice_Command]
                @Action VARCHAR(6),
                @InvoiceId INT = NULL,
                @TaxId VARCHAR(8) = NULL,
                @ClientShortName NVARCHAR(50) = NULL,
                @ElectronicInvoice BIT = NULL,
                @CashRegister BIT = NULL,
                @ThreeCashRegister BIT = NULL,
                @TwoPartInvoice BIT = NULL,
                @TwoPartInvoiceCopy BIT = NULL,
                @ThreePartInvoice BIT = NULL,
                @ThreePartInvoiceCopy BIT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                SET @Action = UPPER(LTRIM(RTRIM(@Action)));

                IF @Action IS NULL OR @Action NOT IN ('CREATE', 'UPDATE', 'DELETE')
                    THROW 53000, 'Unsupported invoice action.', 1;

                IF @Action IN ('CREATE', 'UPDATE')
                BEGIN
                    SET @TaxId = NULLIF(LTRIM(RTRIM(@TaxId)), '');
                    SET @ClientShortName = NULLIF(LTRIM(RTRIM(@ClientShortName)), N'');
                    SET @ElectronicInvoice = ISNULL(@ElectronicInvoice, 0);
                    SET @CashRegister = ISNULL(@CashRegister, 0);
                    SET @ThreeCashRegister = ISNULL(@ThreeCashRegister, 0);
                    SET @TwoPartInvoice = ISNULL(@TwoPartInvoice, 0);
                    SET @TwoPartInvoiceCopy = ISNULL(@TwoPartInvoiceCopy, 0);
                    SET @ThreePartInvoice = ISNULL(@ThreePartInvoice, 0);
                    SET @ThreePartInvoiceCopy = ISNULL(@ThreePartInvoiceCopy, 0);

                    IF @TaxId IS NULL
                        OR LEN(@TaxId) <> 8
                        OR @TaxId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 53001, 'Invoice tax ID is invalid.', 1;

                    IF @ClientShortName IS NULL
                        THROW 53002, 'Invoice client short name is required.', 1;
                END;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    IF @Action = 'CREATE'
                    BEGIN
                        INSERT INTO [dbo].[Invoices]
                        (
                            [TaxId],
                            [ClientShortName],
                            [ElectronicInvoice],
                            [CashRegister],
                            [ThreeCashRegister],
                            [TwoPartInvoice],
                            [TwoPartInvoiceCopy],
                            [ThreePartInvoice],
                            [ThreePartInvoiceCopy]
                        )
                        VALUES
                        (
                            @TaxId,
                            @ClientShortName,
                            @ElectronicInvoice,
                            @CashRegister,
                            @ThreeCashRegister,
                            @TwoPartInvoice,
                            @TwoPartInvoiceCopy,
                            @ThreePartInvoice,
                            @ThreePartInvoiceCopy
                        );

                        SET @InvoiceId = CONVERT(INT, SCOPE_IDENTITY());
                    END
                    ELSE IF @Action = 'UPDATE'
                    BEGIN
                        IF @InvoiceId IS NULL
                            THROW 53006, 'Invoice ID is required for update.', 1;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM [dbo].[Invoices]
                            WHERE [InvoiceId] = @InvoiceId
                        )
                            THROW 53007, 'Invoice not found.', 1;

                        UPDATE [dbo].[Invoices]
                        SET
                            [TaxId] = @TaxId,
                            [ClientShortName] = @ClientShortName,
                            [ElectronicInvoice] = @ElectronicInvoice,
                            [CashRegister] = @CashRegister,
                            [ThreeCashRegister] = @ThreeCashRegister,
                            [TwoPartInvoice] = @TwoPartInvoice,
                            [TwoPartInvoiceCopy] = @TwoPartInvoiceCopy,
                            [ThreePartInvoice] = @ThreePartInvoice,
                            [ThreePartInvoiceCopy] = @ThreePartInvoiceCopy
                        WHERE [InvoiceId] = @InvoiceId;
                    END
                    ELSE
                    BEGIN
                        IF @InvoiceId IS NULL
                            THROW 53008, 'Invoice ID is required for delete.', 1;

                        DELETE FROM [dbo].[Invoices]
                        WHERE [InvoiceId] = @InvoiceId;

                        IF @@ROWCOUNT = 0
                            THROW 53007, 'Invoice not found.', 1;
                    END;

                    COMMIT TRANSACTION;

                    IF @Action IN ('CREATE', 'UPDATE')
                    BEGIN
                        SELECT
                            [InvoiceId],
                            [TaxId],
                            [ClientShortName],
                            [ElectronicInvoice],
                            [CashRegister],
                            [ThreeCashRegister],
                            [TwoPartInvoice],
                            [TwoPartInvoiceCopy],
                            [ThreePartInvoice],
                            [ThreePartInvoiceCopy]
                        FROM [dbo].[Invoices]
                        WHERE [InvoiceId] = @InvoiceId;
                    END;
                END TRY
                BEGIN CATCH
                    IF XACT_STATE() <> 0
                        ROLLBACK TRANSACTION;

                    THROW;
                END CATCH;
            END;
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Invoice_Command]', N'P') IS NOT NULL
                DROP PROCEDURE [dbo].[Invoice_Command];
            """);

        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Invoice_Query]', N'P') IS NOT NULL
                DROP PROCEDURE [dbo].[Invoice_Query];
            """);
    }
}
