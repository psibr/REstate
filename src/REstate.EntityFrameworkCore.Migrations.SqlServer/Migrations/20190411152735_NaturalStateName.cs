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

                migrationBuilder.Sql(@"CREATE NONCLUSTERED INDEX IX_Machines_SchematicName_NaturalStateName_UpdatedTime
ON [dbo].[Machines] ([SchematicName] ASC, [UpdatedTime] ASC, [NaturalStateName] ASC)
INCLUDE ([MachineId])");
            }
            else
            {
                migrationBuilder.CreateIndex(
                    name: "IX_Machines_SchematicName_NaturalStateName_UpdatedTime",
                    table: "Machines",
                    columns: new[] { "SchematicName", "NaturalStateName", "UpdatedTime", "MachineId" });
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
