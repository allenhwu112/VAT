using FluentMigrator;

namespace Vat.Migrations;

[Migration(202608220004L)]
public sealed class CreateEmployeeContactDetails : Migration
{
    public const string ContactPhoneColumnName = "ContactPhone";
    public const string AddressColumnName = "Address";
    public const string BirthDateColumnName = "BirthDate";

    public override void Up()
    {
        Alter.Table("Employees")
            .InSchema("dbo")
            .AddColumn(ContactPhoneColumnName).AsString(30).Nullable()
            .AddColumn(AddressColumnName).AsString(255).Nullable()
            .AddColumn(BirthDateColumnName).AsDate().Nullable();

        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Employee_Query]
                @EmployeeId INT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [EmployeeId],
                    [Name],
                    [ShortName],
                    [Gender],
                    [NationalId],
                    [ContactPhone],
                    [Address],
                    [BirthDate]
                FROM [dbo].[Employees]
                WHERE @EmployeeId IS NULL OR [EmployeeId] = @EmployeeId
                ORDER BY [EmployeeId] DESC;
            END;
            """);

        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Employee_Command]
                @Action VARCHAR(6),
                @EmployeeId INT = NULL,
                @Name NVARCHAR(100) = NULL,
                @ShortName NVARCHAR(50) = NULL,
                @Gender CHAR(1) = NULL,
                @NationalId VARCHAR(10) = NULL,
                @Password NVARCHAR(255) = NULL,
                @ContactPhone NVARCHAR(30) = NULL,
                @Address NVARCHAR(255) = NULL,
                @BirthDate DATE = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                SET @Action = UPPER(LTRIM(RTRIM(@Action)));

                IF @Action IS NULL OR @Action NOT IN ('CREATE', 'UPDATE', 'DELETE')
                    THROW 51000, 'Unsupported employee action.', 1;

                IF @Action IN ('CREATE', 'UPDATE')
                BEGIN
                    SET @Name = NULLIF(LTRIM(RTRIM(@Name)), N'');
                    SET @ShortName = NULLIF(LTRIM(RTRIM(@ShortName)), N'');
                    SET @Gender = NULLIF(UPPER(LTRIM(RTRIM(@Gender))), '');
                    SET @NationalId = NULLIF(UPPER(LTRIM(RTRIM(@NationalId))), '');
                    SET @ContactPhone = NULLIF(LTRIM(RTRIM(@ContactPhone)), N'');
                    SET @Address = NULLIF(LTRIM(RTRIM(@Address)), N'');

                    IF @Name IS NULL
                        THROW 51001, 'Employee name is required.', 1;

                    IF @ShortName IS NULL
                        THROW 51002, 'Employee short name is required.', 1;

                    IF @Gender IS NULL OR @Gender NOT IN ('M', 'F')
                        THROW 51003, 'Employee gender is invalid.', 1;

                    IF @NationalId IS NULL
                        OR LEN(@NationalId) <> 10
                        OR @NationalId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 51004, 'Employee national ID is invalid.', 1;
                END;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    IF @Action = 'CREATE'
                    BEGIN
                        IF NULLIF(@Password, N'') IS NULL
                            THROW 51005, 'Employee password is required.', 1;

                        INSERT INTO [dbo].[Employees]
                        (
                            [Name],
                            [ShortName],
                            [Gender],
                            [NationalId],
                            [Password],
                            [ContactPhone],
                            [Address],
                            [BirthDate]
                        )
                        VALUES
                        (
                            @Name,
                            @ShortName,
                            @Gender,
                            @NationalId,
                            @Password,
                            @ContactPhone,
                            @Address,
                            @BirthDate
                        );

                        SET @EmployeeId = CONVERT(INT, SCOPE_IDENTITY());
                    END
                    ELSE IF @Action = 'UPDATE'
                    BEGIN
                        IF @EmployeeId IS NULL
                            THROW 51006, 'Employee ID is required for update.', 1;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM [dbo].[Employees]
                            WHERE [EmployeeId] = @EmployeeId
                        )
                            THROW 51007, 'Employee not found.', 1;

                        UPDATE [dbo].[Employees]
                        SET
                            [Name] = @Name,
                            [ShortName] = @ShortName,
                            [Gender] = @Gender,
                            [NationalId] = @NationalId,
                            [Password] = CASE
                                WHEN NULLIF(@Password, N'') IS NULL THEN [Password]
                                ELSE @Password
                            END,
                            [ContactPhone] = @ContactPhone,
                            [Address] = @Address,
                            [BirthDate] = @BirthDate
                        WHERE [EmployeeId] = @EmployeeId;
                    END
                    ELSE
                    BEGIN
                        IF @EmployeeId IS NULL
                            THROW 51008, 'Employee ID is required for delete.', 1;

                        DELETE FROM [dbo].[Employees]
                        WHERE [EmployeeId] = @EmployeeId;

                        IF @@ROWCOUNT = 0
                            THROW 51007, 'Employee not found.', 1;
                    END;

                    COMMIT TRANSACTION;

                    IF @Action IN ('CREATE', 'UPDATE')
                    BEGIN
                        SELECT
                            [EmployeeId],
                            [Name],
                            [ShortName],
                            [Gender],
                            [NationalId],
                            [ContactPhone],
                            [Address],
                            [BirthDate]
                        FROM [dbo].[Employees]
                        WHERE [EmployeeId] = @EmployeeId;
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
            CREATE OR ALTER PROCEDURE [dbo].[Employee_Query]
                @EmployeeId INT = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [EmployeeId],
                    [Name],
                    [ShortName],
                    [Gender],
                    [NationalId]
                FROM [dbo].[Employees]
                WHERE @EmployeeId IS NULL OR [EmployeeId] = @EmployeeId
                ORDER BY [EmployeeId] DESC;
            END;
            """);

        Execute.Sql("""
            CREATE OR ALTER PROCEDURE [dbo].[Employee_Command]
                @Action VARCHAR(6),
                @EmployeeId INT = NULL,
                @Name NVARCHAR(100) = NULL,
                @ShortName NVARCHAR(50) = NULL,
                @Gender CHAR(1) = NULL,
                @NationalId VARCHAR(10) = NULL,
                @Password NVARCHAR(255) = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                SET @Action = UPPER(LTRIM(RTRIM(@Action)));

                IF @Action IS NULL OR @Action NOT IN ('CREATE', 'UPDATE', 'DELETE')
                    THROW 51000, 'Unsupported employee action.', 1;

                IF @Action IN ('CREATE', 'UPDATE')
                BEGIN
                    SET @Name = NULLIF(LTRIM(RTRIM(@Name)), N'');
                    SET @ShortName = NULLIF(LTRIM(RTRIM(@ShortName)), N'');
                    SET @Gender = NULLIF(UPPER(LTRIM(RTRIM(@Gender))), '');
                    SET @NationalId = NULLIF(UPPER(LTRIM(RTRIM(@NationalId))), '');

                    IF @Name IS NULL
                        THROW 51001, 'Employee name is required.', 1;

                    IF @ShortName IS NULL
                        THROW 51002, 'Employee short name is required.', 1;

                    IF @Gender IS NULL OR @Gender NOT IN ('M', 'F')
                        THROW 51003, 'Employee gender is invalid.', 1;

                    IF @NationalId IS NULL
                        OR LEN(@NationalId) <> 10
                        OR @NationalId COLLATE Latin1_General_100_BIN2 NOT LIKE
                            '[A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                        THROW 51004, 'Employee national ID is invalid.', 1;
                END;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    IF @Action = 'CREATE'
                    BEGIN
                        IF NULLIF(@Password, N'') IS NULL
                            THROW 51005, 'Employee password is required.', 1;

                        INSERT INTO [dbo].[Employees]
                        (
                            [Name],
                            [ShortName],
                            [Gender],
                            [NationalId],
                            [Password]
                        )
                        VALUES
                        (
                            @Name,
                            @ShortName,
                            @Gender,
                            @NationalId,
                            @Password
                        );

                        SET @EmployeeId = CONVERT(INT, SCOPE_IDENTITY());
                    END
                    ELSE IF @Action = 'UPDATE'
                    BEGIN
                        IF @EmployeeId IS NULL
                            THROW 51006, 'Employee ID is required for update.', 1;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM [dbo].[Employees]
                            WHERE [EmployeeId] = @EmployeeId
                        )
                            THROW 51007, 'Employee not found.', 1;

                        UPDATE [dbo].[Employees]
                        SET
                            [Name] = @Name,
                            [ShortName] = @ShortName,
                            [Gender] = @Gender,
                            [NationalId] = @NationalId,
                            [Password] = CASE
                                WHEN NULLIF(@Password, N'') IS NULL THEN [Password]
                                ELSE @Password
                            END
                        WHERE [EmployeeId] = @EmployeeId;
                    END
                    ELSE
                    BEGIN
                        IF @EmployeeId IS NULL
                            THROW 51008, 'Employee ID is required for delete.', 1;

                        DELETE FROM [dbo].[Employees]
                        WHERE [EmployeeId] = @EmployeeId;

                        IF @@ROWCOUNT = 0
                            THROW 51007, 'Employee not found.', 1;
                    END;

                    COMMIT TRANSACTION;

                    IF @Action IN ('CREATE', 'UPDATE')
                    BEGIN
                        SELECT
                            [EmployeeId],
                            [Name],
                            [ShortName],
                            [Gender],
                            [NationalId]
                        FROM [dbo].[Employees]
                        WHERE [EmployeeId] = @EmployeeId;
                    END;
                END TRY
                BEGIN CATCH
                    IF XACT_STATE() <> 0
                        ROLLBACK TRANSACTION;

                    THROW;
                END CATCH;
            END;
            """);

        Delete.Column(BirthDateColumnName).FromTable("Employees").InSchema("dbo");
        Delete.Column(AddressColumnName).FromTable("Employees").InSchema("dbo");
        Delete.Column(ContactPhoneColumnName).FromTable("Employees").InSchema("dbo");
    }
}
