using FluentMigrator;

namespace Vat.Migrations;

[Migration(202608220002L)]
public sealed class CreateEmployeesTable : Migration
{
    public override void Up()
    {
        Create.Table("Employees")
            .InSchema("dbo")
            .WithColumn("EmployeeId").AsInt32().Identity().NotNullable().PrimaryKey("PK_Employees")
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("ShortName").AsString(50).NotNullable()
            .WithColumn("Gender").AsFixedLengthAnsiString(1).NotNullable()
            .WithColumn("NationalId").AsAnsiString(10).NotNullable()
            .WithColumn("Password").AsString(255).NotNullable();

        Create.Index("UX_Employees_NationalId")
            .OnTable("Employees")
            .InSchema("dbo")
            .WithOptions()
            .Unique()
            .OnColumn("NationalId").Ascending();

        Execute.Sql("""
            ALTER TABLE [dbo].[Employees]
            ADD CONSTRAINT [CK_Employees_Gender]
                CHECK ([Gender] IN ('M', 'F'));

            ALTER TABLE [dbo].[Employees]
            ADD CONSTRAINT [CK_Employees_NationalId]
                CHECK (
                    LEN([NationalId]) = 10
                    AND [NationalId] COLLATE Latin1_General_100_BIN2 LIKE
                        '[A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                );
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Employees]', N'U') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Employees] DROP CONSTRAINT IF EXISTS [CK_Employees_NationalId];
                ALTER TABLE [dbo].[Employees] DROP CONSTRAINT IF EXISTS [CK_Employees_Gender];
            END;
            """);

        Delete.Index("UX_Employees_NationalId")
            .OnTable("Employees")
            .InSchema("dbo");

        Delete.Table("Employees").InSchema("dbo");
    }
}
