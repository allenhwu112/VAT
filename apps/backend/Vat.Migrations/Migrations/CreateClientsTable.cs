using FluentMigrator;

namespace Vat.Migrations;

[Migration(202609060001L)]
public sealed class CreateClientsTable : Migration
{
    public const string ClientIdColumnName = "ClientId";
    public const string TaxIdColumnName = "TaxId";
    public const string FullNameColumnName = "FullName";
    public const string ShortNameColumnName = "ShortName";
    public const string ResponsiblePersonColumnName = "ResponsiblePerson";
    public const string AddressColumnName = "Address";

    public override void Up()
    {
        Create.Table("Clients")
            .InSchema("dbo")
            .WithColumn(ClientIdColumnName).AsInt32().Identity().NotNullable().PrimaryKey("PK_Clients")
            .WithColumn(TaxIdColumnName).AsAnsiString(8).NotNullable()
            .WithColumn(FullNameColumnName).AsString(100).NotNullable()
            .WithColumn(ShortNameColumnName).AsString(50).NotNullable()
            .WithColumn(ResponsiblePersonColumnName).AsString(100).NotNullable()
            .WithColumn(AddressColumnName).AsString(255).NotNullable();

        Create.Index("UX_Clients_TaxId")
            .OnTable("Clients")
            .InSchema("dbo")
            .WithOptions()
            .Unique()
            .OnColumn(TaxIdColumnName).Ascending();

        Execute.Sql("""
            ALTER TABLE [dbo].[Clients]
            ADD CONSTRAINT [CK_Clients_TaxId]
                CHECK (
                    LEN([TaxId]) = 8
                    AND [TaxId] COLLATE Latin1_General_100_BIN2 LIKE
                        '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
                );

            ALTER TABLE [dbo].[Clients]
            ADD CONSTRAINT [CK_Clients_RequiredText]
                CHECK (
                    LEN(LTRIM(RTRIM([FullName]))) > 0
                    AND LEN(LTRIM(RTRIM([ShortName]))) > 0
                    AND LEN(LTRIM(RTRIM([ResponsiblePerson]))) > 0
                    AND LEN(LTRIM(RTRIM([Address]))) > 0
                );
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            IF OBJECT_ID(N'[dbo].[Clients]', N'U') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Clients] DROP CONSTRAINT IF EXISTS [CK_Clients_RequiredText];
                ALTER TABLE [dbo].[Clients] DROP CONSTRAINT IF EXISTS [CK_Clients_TaxId];
            END;
            """);

        Delete.Index("UX_Clients_TaxId")
            .OnTable("Clients")
            .InSchema("dbo");

        Delete.Table("Clients").InSchema("dbo");
    }
}
