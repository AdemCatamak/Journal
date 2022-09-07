using FluentMigrator;

namespace Journal.Infra.DataStorage.Migrations;

[Migration(1)]
public class M0001CreateAuditLogTable : Migration
{
    public override void Up()
    {
        Create.Table("audit_logs")
              .WithColumn("id").AsInt64().Identity()
              .WithColumn("server_name").AsString().NotNullable()
              .WithColumn("db_name").AsString().NotNullable()
              .WithColumn("table_name").AsString().NotNullable()
              .WithColumn("row_id").AsString().NotNullable()
              .WithColumn("column_name").AsString().NotNullable()
              .WithColumn("old_value").AsString().Nullable()
              .WithColumn("new_value").AsString().NotNullable()
              .WithColumn("executed_by").AsString().NotNullable()
              .WithColumn("executed_on").AsDateTimeOffset().NotNullable()
              .WithColumn("created_on").AsDateTimeOffset().NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("audit_logs");
    }
}