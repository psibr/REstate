using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace REstate.EntityFrameworkCore.Migrations.SqlServer.Migrations
{
    public partial class _900preview9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<string>(unicode: false, nullable: false),
                    CommitNumber = table.Column<long>(nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(nullable: false),
                    StateJson = table.Column<string>(unicode: false, nullable: false),
                    SchematicBytes = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineId);
                });

            migrationBuilder.CreateTable(
                name: "Schematics",
                columns: table => new
                {
                    SchematicName = table.Column<string>(unicode: false, nullable: false),
                    SchematicBytes = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schematics", x => x.SchematicName);
                });

            migrationBuilder.CreateTable(
                name: "MetadataEntries",
                columns: table => new
                {
                    MachineId = table.Column<string>(unicode: false, nullable: false),
                    Key = table.Column<string>(unicode: false, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataEntries", x => new { x.MachineId, x.Key });
                    table.ForeignKey(
                        name: "FK_MetadataEntries_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StateBagEntries",
                columns: table => new
                {
                    MachineId = table.Column<string>(unicode: false, nullable: false),
                    Key = table.Column<string>(unicode: false, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateBagEntries", x => new { x.MachineId, x.Key });
                    table.ForeignKey(
                        name: "FK_StateBagEntries_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetadataEntries");

            migrationBuilder.DropTable(
                name: "Schematics");

            migrationBuilder.DropTable(
                name: "StateBagEntries");

            migrationBuilder.DropTable(
                name: "Machines");
        }
    }
}
