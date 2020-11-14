using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class time_to_find_index_process : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeToFindFrame",
                table: "Processes");

            migrationBuilder.AddColumn<long>(
                name: "TimeToFindIndex",
                table: "Processes",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeToFindIndex",
                table: "Processes");

            migrationBuilder.AddColumn<double>(
                name: "TimeToFindFrame",
                table: "Processes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
