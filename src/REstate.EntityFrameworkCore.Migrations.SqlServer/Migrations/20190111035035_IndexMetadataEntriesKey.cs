using Microsoft.EntityFrameworkCore.Migrations;

namespace REstate.EntityFrameworkCore.Migrations.SqlServer.Migrations
{
    public partial class IndexMetadataEntriesKey : Migration
    {
        private const string SqlServer = "Microsoft.EntityFrameworkCore.SqlServer";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == SqlServer)
            {
                migrationBuilder.Sql(@"CREATE NONCLUSTERED INDEX IX_MetadataEntries_Key
ON [dbo].[MetadataEntries] ([Key])
INCLUDE ([Value])");
            }
            else
            {
                migrationBuilder.CreateIndex(
                    name: "IX_MetadataEntries_Key",
                    table: "MetadataEntries",
                    column: "Key");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetadataEntries_Key",
                table: "MetadataEntries");
        }
    }
}
