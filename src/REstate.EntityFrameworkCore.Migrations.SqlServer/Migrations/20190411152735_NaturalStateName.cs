using Microsoft.EntityFrameworkCore.Migrations;

namespace REstate.EntityFrameworkCore.Migrations.SqlServer.Migrations
{
    public partial class NaturalStateName : Migration
    {
        private const string SqlServer = "Microsoft.EntityFrameworkCore.SqlServer";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NaturalStateName",
                table: "Machines",
                unicode: false,
                maxLength: 450,
                nullable: true);

            if (migrationBuilder.ActiveProvider == SqlServer)
            {
                migrationBuilder.Sql(@"UPDATE [dbo].[Machines] SET [NaturalStateName] = CAST(LEFT(JSON_VALUE([StateJson],'$.AssemblyQualifiedName'), CHARINDEX(',', JSON_VALUE([StateJson],'$.AssemblyQualifiedName')) - 1) AS VARCHAR(450))");

                migrationBuilder.Sql(@"CREATE NONCLUSTERED INDEX IX_Machines_SchematicName_NaturalStateName_UpdatedTime
ON [dbo].[Machines] ([SchematicName], [UpdatedTime], [NaturalStateName])
INCLUDE ([MachineId], [CommitNumber], [StateJson], [SchematicBytes])");
            }
            else
            {
                migrationBuilder.CreateIndex(
                    name: "IX_Machines_SchematicName_NaturalStateName_UpdatedTime",
                    table: "Machines",
                    columns: new[] { "SchematicName", "NaturalStateName", "UpdatedTime" });
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Machines_SchematicName_NaturalStateName_UpdatedTime",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "NaturalStateName",
                table: "Machines");
        }
    }
}
