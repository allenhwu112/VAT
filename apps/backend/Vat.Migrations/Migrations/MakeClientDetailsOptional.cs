using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609120001L)]
public sealed class MakeClientDetailsOptional : Migration
{
    public override void Up()
    {
        // SQL Server requires explicit ALTER COLUMN and constraint replacement
        // to make the deployed client detail columns nullable safely.
        Execute.Sql("""
            ALTER TABLE [dbo].[Clients]
                DROP CONSTRAINT IF EXISTS [CK_Clients_RequiredText];

            ALTER TABLE [dbo].[Clients] ALTER COLUMN [FullName] NVARCHAR(100) NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [ShortName] NVARCHAR(50) NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [ResponsiblePerson] NVARCHAR(100) NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [Address] NVARCHAR(255) NULL;

            ALTER TABLE [dbo].[Clients]
                ADD CONSTRAINT [CK_Clients_OptionalText]
                CHECK (
                    ([FullName] IS NULL OR LEN(LTRIM(RTRIM([FullName]))) > 0)
                    AND ([ShortName] IS NULL OR LEN(LTRIM(RTRIM([ShortName]))) > 0)
                    AND ([ResponsiblePerson] IS NULL OR LEN(LTRIM(RTRIM([ResponsiblePerson]))) > 0)
                    AND ([Address] IS NULL OR LEN(LTRIM(RTRIM([Address]))) > 0)
                );
            """);

        // The existing stored procedure is an applied migration, so recreate it
        // here instead of editing the historical migration file.
        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Client_Command]
                @Action VARCHAR(6),
                @ClientId INT = NULL,
                @TaxId VARCHAR(8) = NULL,
                @FullName NVARCHAR(100) = NULL,
                @ShortName NVARCHAR(50) = NULL,
                @ResponsiblePerson NVARCHAR(100) = NULL,
                @Address NVARCHAR(255) = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                SET @Action = UPPER(LTRIM(RTRIM(@Action)));

                IF @Action IS NULL OR @Action NOT IN ('CREATE', 'UPDATE', 'DELETE')
                    THROW 52000, 'Unsupported client action.', 1;

                IF @Action IN ('CREATE', 'UPDATE')
                BEGIN
                    SET @TaxId = NULLIF(LTRIM(RTRIM(@TaxId)), '');
                    SET @FullName = NULLIF(LTRIM(RTRIM(@FullName)), N'');
                    SET @ShortName = NULLIF(LTRIM(RTRIM(@ShortName)), N'');
                    SET @ResponsiblePerson = NULLIF(LTRIM(RTRIM(@ResponsiblePerson)), N'');
                    SET @Address = NULLIF(LTRIM(RTRIM(@Address)), N'');

                    IF @TaxId IS NULL
                        OR LEN(@TaxId) <> 8
                        OR @TaxId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 52001, 'Client tax ID is invalid.', 1;
                END;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    IF @Action = 'CREATE'
                    BEGIN
                        INSERT INTO [dbo].[Clients]
                        (
                            [TaxId],
                            [FullName],
                            [ShortName],
                            [ResponsiblePerson],
                            [Address]
                        )
                        VALUES
                        (
                            @TaxId,
                            @FullName,
                            @ShortName,
                            @ResponsiblePerson,
                            @Address
                        );

                        SET @ClientId = CONVERT(INT, SCOPE_IDENTITY());
                    END
                    ELSE IF @Action = 'UPDATE'
                    BEGIN
                        IF @ClientId IS NULL
                            THROW 52006, 'Client ID is required for update.', 1;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM [dbo].[Clients]
                            WHERE [ClientId] = @ClientId
                        )
                            THROW 52007, 'Client not found.', 1;

                        UPDATE [dbo].[Clients]
                        SET
                            [TaxId] = @TaxId,
                            [FullName] = @FullName,
                            [ShortName] = @ShortName,
                            [ResponsiblePerson] = @ResponsiblePerson,
                            [Address] = @Address
                        WHERE [ClientId] = @ClientId;
                    END
                    ELSE
                    BEGIN
                        IF @ClientId IS NULL
                            THROW 52008, 'Client ID is required for delete.', 1;

                        DELETE FROM [dbo].[Clients]
                        WHERE [ClientId] = @ClientId;

                        IF @@ROWCOUNT = 0
                            THROW 52007, 'Client not found.', 1;
                    END;

                    COMMIT TRANSACTION;

                    IF @Action IN ('CREATE', 'UPDATE')
                    BEGIN
                        SELECT
                            [ClientId],
                            [TaxId],
                            [FullName],
                            [ShortName],
                            [ResponsiblePerson],
                            [Address]
                        FROM [dbo].[Clients]
                        WHERE [ClientId] = @ClientId;
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
        // Do not make nullable data fail halfway through a rollback.
        Execute.Sql("""
            IF EXISTS
            (
                SELECT 1
                FROM [dbo].[Clients]
                WHERE [FullName] IS NULL
                   OR [ShortName] IS NULL
                   OR [ResponsiblePerson] IS NULL
                   OR [Address] IS NULL
            )
                THROW 52010, 'Cannot rollback client details while NULL values exist.', 1;

            ALTER TABLE [dbo].[Clients]
                DROP CONSTRAINT IF EXISTS [CK_Clients_OptionalText];

            ALTER TABLE [dbo].[Clients] ALTER COLUMN [FullName] NVARCHAR(100) NOT NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [ShortName] NVARCHAR(50) NOT NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [ResponsiblePerson] NVARCHAR(100) NOT NULL;
            ALTER TABLE [dbo].[Clients] ALTER COLUMN [Address] NVARCHAR(255) NOT NULL;

            ALTER TABLE [dbo].[Clients]
                ADD CONSTRAINT [CK_Clients_RequiredText]
                CHECK (
                    LEN(LTRIM(RTRIM([FullName]))) > 0
                    AND LEN(LTRIM(RTRIM([ShortName]))) > 0
                    AND LEN(LTRIM(RTRIM([ResponsiblePerson]))) > 0
                    AND LEN(LTRIM(RTRIM([Address]))) > 0
                );
            """);

        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Client_Command]
                @Action VARCHAR(6),
                @ClientId INT = NULL,
                @TaxId VARCHAR(8) = NULL,
                @FullName NVARCHAR(100) = NULL,
                @ShortName NVARCHAR(50) = NULL,
                @ResponsiblePerson NVARCHAR(100) = NULL,
                @Address NVARCHAR(255) = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                SET @Action = UPPER(LTRIM(RTRIM(@Action)));

                IF @Action IS NULL OR @Action NOT IN ('CREATE', 'UPDATE', 'DELETE')
                    THROW 52000, 'Unsupported client action.', 1;

                IF @Action IN ('CREATE', 'UPDATE')
                BEGIN
                    SET @TaxId = NULLIF(LTRIM(RTRIM(@TaxId)), '');
                    SET @FullName = NULLIF(LTRIM(RTRIM(@FullName)), N'');
                    SET @ShortName = NULLIF(LTRIM(RTRIM(@ShortName)), N'');
                    SET @ResponsiblePerson = NULLIF(LTRIM(RTRIM(@ResponsiblePerson)), N'');
                    SET @Address = NULLIF(LTRIM(RTRIM(@Address)), N'');

                    IF @TaxId IS NULL
                        OR LEN(@TaxId) <> 8
                        OR @TaxId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 52001, 'Client tax ID is invalid.', 1;

                    IF @FullName IS NULL
                        THROW 52002, 'Client full name is required.', 1;

                    IF @ShortName IS NULL
                        THROW 52003, 'Client short name is required.', 1;

                    IF @ResponsiblePerson IS NULL
                        THROW 52004, 'Client responsible person is required.', 1;

                    IF @Address IS NULL
                        THROW 52005, 'Client address is required.', 1;
                END;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    IF @Action = 'CREATE'
                    BEGIN
                        INSERT INTO [dbo].[Clients]
                        (
                            [TaxId],
                            [FullName],
                            [ShortName],
                            [ResponsiblePerson],
                            [Address]
                        )
                        VALUES
                        (
                            @TaxId,
                            @FullName,
                            @ShortName,
                            @ResponsiblePerson,
                            @Address
                        );

                        SET @ClientId = CONVERT(INT, SCOPE_IDENTITY());
                    END
                    ELSE IF @Action = 'UPDATE'
                    BEGIN
                        IF @ClientId IS NULL
                            THROW 52006, 'Client ID is required for update.', 1;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM [dbo].[Clients]
                            WHERE [ClientId] = @ClientId
                        )
                            THROW 52007, 'Client not found.', 1;

                        UPDATE [dbo].[Clients]
                        SET
                            [TaxId] = @TaxId,
                            [FullName] = @FullName,
                            [ShortName] = @ShortName,
                            [ResponsiblePerson] = @ResponsiblePerson,
                            [Address] = @Address
                        WHERE [ClientId] = @ClientId;
                    END
                    ELSE
                    BEGIN
                        IF @ClientId IS NULL
                            THROW 52008, 'Client ID is required for delete.', 1;

                        DELETE FROM [dbo].[Clients]
                        WHERE [ClientId] = @ClientId;

                        IF @@ROWCOUNT = 0
                            THROW 52007, 'Client not found.', 1;
                    END;

                    COMMIT TRANSACTION;

                    IF @Action IN ('CREATE', 'UPDATE')
                    BEGIN
                        SELECT
                            [ClientId],
                            [TaxId],
                            [FullName],
                            [ShortName],
                            [ResponsiblePerson],
                            [Address]
                        FROM [dbo].[Clients]
                        WHERE [ClientId] = @ClientId;
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
