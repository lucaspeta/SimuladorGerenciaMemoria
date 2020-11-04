using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class memory_initial_script_controller : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Processes",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsInitial",
                table: "Frames",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "RegL",
                table: "Frames",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "TipoAlg",
                table: "Frames",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInitial",
                table: "Frames");

            migrationBuilder.DropColumn(
                name: "RegL",
                table: "Frames");

            migrationBuilder.DropColumn(
                name: "TipoAlg",
                table: "Frames");

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Processes",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
