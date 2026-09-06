using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609060002L)]
public sealed class CreateClientProcedures : Migration
{
    public const string QueryProcedureName = "Client_Query";
    public const string CommandProcedureName = "Client_Command";

    public override void Up()
    {
        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Client_Query]
                @ClientId INT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [ClientId],
                    [TaxId],
                    [FullName],
                    [ShortName],
                    [ResponsiblePerson],
                    [Address]
                FROM [dbo].[Clients]
                WHERE @ClientId IS NULL OR [ClientId] = @ClientId
                ORDER BY [ClientId] DESC;
            END;
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

    public override void Down()
    {
        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Client_Command]', N'P') IS NOT NULL
                DROP PROCEDURE [dbo].[Client_Command];
            """);

        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Client_Query]', N'P') IS NOT NULL
                DROP PROCEDURE [dbo].[Client_Query];
            """);
    }
}
