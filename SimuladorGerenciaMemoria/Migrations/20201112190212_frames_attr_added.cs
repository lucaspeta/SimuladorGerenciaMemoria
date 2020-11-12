using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class frames_attr_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegL",
                table: "Frames");

            migrationBuilder.AddColumn<int>(
                name: "CapacidadeUtilizada",
                table: "Frames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FrameSize",
                table: "Frames",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacidadeUtilizada",
                table: "Frames");

            migrationBuilder.DropColumn(
                name: "FrameSize",
                table: "Frames");

            migrationBuilder.AddColumn<long>(
                name: "RegL",
                table: "Frames",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
