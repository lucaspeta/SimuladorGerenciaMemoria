using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class memorie_frame_process_fk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Frames_Memories_MemoryID",
                table: "Frames");

            migrationBuilder.DropForeignKey(
                name: "FK_Frames_Processes_ProcessID",
                table: "Frames");

            migrationBuilder.DropColumn(
                name: "RegB",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "RegL",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "TimeToFindIndex",
                table: "Processes");

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Processes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ProcessID",
                table: "Frames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MemoryID",
                table: "Frames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Frames_Memories_MemoryID",
                table: "Frames",
                column: "MemoryID",
                principalTable: "Memories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Frames_Processes_ProcessID",
                table: "Frames",
                column: "ProcessID",
                principalTable: "Processes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Frames_Memories_MemoryID",
                table: "Frames");

            migrationBuilder.DropForeignKey(
                name: "FK_Frames_Processes_ProcessID",
                table: "Frames");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Processes");

            migrationBuilder.AddColumn<long>(
                name: "RegB",
                table: "Processes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RegL",
                table: "Processes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "TimeToFindIndex",
                table: "Processes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "ProcessID",
                table: "Frames",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "MemoryID",
                table: "Frames",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Frames_Memories_MemoryID",
                table: "Frames",
                column: "MemoryID",
                principalTable: "Memories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Frames_Processes_ProcessID",
                table: "Frames",
                column: "ProcessID",
                principalTable: "Processes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
