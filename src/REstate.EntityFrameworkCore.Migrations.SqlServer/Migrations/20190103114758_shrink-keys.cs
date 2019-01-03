using Microsoft.EntityFrameworkCore.Migrations;

namespace REstate.EntityFrameworkCore.Migrations.SqlServer.Migrations
{
    public partial class ShrinkKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetadataEntries_Machines_MachineId",
                table: "MetadataEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_StateBagEntries_Machines_MachineId",
                table: "StateBagEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Machines",
                table: "Machines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetadataEntries",
                table: "MetadataEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateBagEntries",
                table: "StateBagEntries");

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "Machines",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "StateBagEntries",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "StateBagEntries",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "MetadataEntries",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "MetadataEntries",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Machines",
                table: "Machines",
                column: "MachineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetadataEntries",
                table: "MetadataEntries",
                columns: new[] { "MachineId", "Key" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateBagEntries",
                table: "StateBagEntries",
                columns: new[] { "MachineId", "Key" });
            
            migrationBuilder.AddForeignKey(
                name: "FK_MetadataEntries_Machines_MachineId",
                table: "MetadataEntries",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateBagEntries_Machines_MachineId",
                table: "StateBagEntries",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetadataEntries_Machines_MachineId",
                table: "MetadataEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_StateBagEntries_Machines_MachineId",
                table: "StateBagEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Machines",
                table: "Machines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetadataEntries",
                table: "MetadataEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StateBagEntries",
                table: "StateBagEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "StateBagEntries",
                unicode: false,
                nullable: false,
                maxLength: 900,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "StateBagEntries",
                unicode: false,
                nullable: false,
                maxLength: 900,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "MetadataEntries",
                unicode: false,
                nullable: false,
                maxLength: 900,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "MetadataEntries",
                unicode: false,
                nullable: false,
                maxLength: 900,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "Machines",
                unicode: false,
                nullable: false,
                maxLength: 900,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Machines",
                table: "Machines",
                column: "MachineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetadataEntries",
                table: "MetadataEntries",
                columns: new[] { "MachineId", "Key" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateBagEntries",
                table: "StateBagEntries",
                columns: new[] { "MachineId", "Key" });

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataEntries_Machines_MachineId",
                table: "MetadataEntries",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateBagEntries_Machines_MachineId",
                table: "StateBagEntries",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
