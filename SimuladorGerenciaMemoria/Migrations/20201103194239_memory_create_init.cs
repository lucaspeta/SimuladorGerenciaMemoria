using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class memory_create_init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Simulations",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemoryID",
                table: "Processes",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Memories",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isGeneratedProcessList",
                table: "Memories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Processes_MemoryID",
                table: "Processes",
                column: "MemoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Processes_Memories_MemoryID",
                table: "Processes",
                column: "MemoryID",
                principalTable: "Memories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Processes_Memories_MemoryID",
                table: "Processes");

            migrationBuilder.DropIndex(
                name: "IX_Processes_MemoryID",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "MemoryID",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "isGeneratedProcessList",
                table: "Memories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Simulations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Memories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
