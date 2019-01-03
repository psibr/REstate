using Microsoft.EntityFrameworkCore.Migrations;

namespace REstate.EntityFrameworkCore.Migrations.SqlServer.Migrations
{
    public partial class AddSchematicNameToMachines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchematicName",
                table: "Machines",
                unicode: false,
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchematicName",
                table: "Machines");
        }
    }
}
