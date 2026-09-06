using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609060005L)]
public sealed class UpdateInvoiceQuantityFields : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_CashRegister];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_ThreeCashRegister];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_TwoPartInvoice];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_TwoPartInvoiceCopy];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_ThreePartInvoice];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [DF_Invoices_ThreePartInvoiceCopy];

            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [CashRegister] INT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreeCashRegister] INT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [TwoPartInvoice] INT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [TwoPartInvoiceCopy] INT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreePartInvoice] INT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreePartInvoiceCopy] INT NOT NULL;

            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_CashRegister_Range]
                CHECK ([CashRegister] BETWEEN 0 AND 99);
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_ThreeCashRegister_Range]
                CHECK ([ThreeCashRegister] BETWEEN 0 AND 99);
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_TwoPartInvoice_Range]
                CHECK ([TwoPartInvoice] BETWEEN 0 AND 99);
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_TwoPartInvoiceCopy_Range]
                CHECK ([TwoPartInvoiceCopy] BETWEEN 0 AND 99);
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_ThreePartInvoice_Range]
                CHECK ([ThreePartInvoice] BETWEEN 0 AND 99);
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [CK_Invoices_ThreePartInvoiceCopy_Range]
                CHECK ([ThreePartInvoiceCopy] BETWEEN 0 AND 99);
            """);

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
                @CashRegister INT = NULL,
                @ThreeCashRegister INT = NULL,
                @TwoPartInvoice INT = NULL,
                @TwoPartInvoiceCopy INT = NULL,
                @ThreePartInvoice INT = NULL,
                @ThreePartInvoiceCopy INT = NULL
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

                    IF @TaxId IS NULL
                        OR LEN(@TaxId) <> 8
                        OR @TaxId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 53001, 'Invoice tax ID is invalid.', 1;

                    IF @ClientShortName IS NULL
                        THROW 53002, 'Invoice client short name is required.', 1;

                    IF @CashRegister IS NULL OR @CashRegister NOT BETWEEN 0 AND 99
                        THROW 53101, 'Cash register quantity must be between 0 and 99.', 1;
                    IF @ThreeCashRegister IS NULL OR @ThreeCashRegister NOT BETWEEN 0 AND 99
                        THROW 53102, 'Three cash register quantity must be between 0 and 99.', 1;
                    IF @TwoPartInvoice IS NULL OR @TwoPartInvoice NOT BETWEEN 0 AND 99
                        THROW 53103, 'Two-part invoice quantity must be between 0 and 99.', 1;
                    IF @TwoPartInvoiceCopy IS NULL OR @TwoPartInvoiceCopy NOT BETWEEN 0 AND 99
                        THROW 53104, 'Two-part invoice copy quantity must be between 0 and 99.', 1;
                    IF @ThreePartInvoice IS NULL OR @ThreePartInvoice NOT BETWEEN 0 AND 99
                        THROW 53105, 'Three-part invoice quantity must be between 0 and 99.', 1;
                    IF @ThreePartInvoiceCopy IS NULL OR @ThreePartInvoiceCopy NOT BETWEEN 0 AND 99
                        THROW 53106, 'Three-part invoice copy quantity must be between 0 and 99.', 1;
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
            IF EXISTS
            (
                SELECT 1
                FROM [dbo].[Invoices]
                WHERE [CashRegister] NOT IN (0, 1)
                   OR [ThreeCashRegister] NOT IN (0, 1)
                   OR [TwoPartInvoice] NOT IN (0, 1)
                   OR [TwoPartInvoiceCopy] NOT IN (0, 1)
                   OR [ThreePartInvoice] NOT IN (0, 1)
                   OR [ThreePartInvoiceCopy] NOT IN (0, 1)
            )
                THROW 53120, 'Cannot rollback invoice quantities greater than 1 to Boolean values.', 1;

            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_CashRegister_Range];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_ThreeCashRegister_Range];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_TwoPartInvoice_Range];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_TwoPartInvoiceCopy_Range];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_ThreePartInvoice_Range];
            ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT IF EXISTS [CK_Invoices_ThreePartInvoiceCopy_Range];

            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [CashRegister] BIT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreeCashRegister] BIT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [TwoPartInvoice] BIT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [TwoPartInvoiceCopy] BIT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreePartInvoice] BIT NOT NULL;
            ALTER TABLE [dbo].[Invoices] ALTER COLUMN [ThreePartInvoiceCopy] BIT NOT NULL;

            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_CashRegister] DEFAULT 0 FOR [CashRegister];
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_ThreeCashRegister] DEFAULT 0 FOR [ThreeCashRegister];
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_TwoPartInvoice] DEFAULT 0 FOR [TwoPartInvoice];
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_TwoPartInvoiceCopy] DEFAULT 0 FOR [TwoPartInvoiceCopy];
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_ThreePartInvoice] DEFAULT 0 FOR [ThreePartInvoice];
            ALTER TABLE [dbo].[Invoices]
                ADD CONSTRAINT [DF_Invoices_ThreePartInvoiceCopy] DEFAULT 0 FOR [ThreePartInvoiceCopy];
            """);

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
}
