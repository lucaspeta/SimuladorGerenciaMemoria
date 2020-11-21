using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class add_new_fields_process : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeToFindIndex",
                table: "Processes");

            migrationBuilder.AddColumn<int>(
                name: "TimeToFindIndexBest",
                table: "Processes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeToFindIndexFirst",
                table: "Processes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeToFindIndexNext",
                table: "Processes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeToFindIndexWorst",
                table: "Processes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeToFindIndexBest",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "TimeToFindIndexFirst",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "TimeToFindIndexNext",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "TimeToFindIndexWorst",
                table: "Processes");

            migrationBuilder.AddColumn<string>(
                name: "TimeToFindIndex",
                table: "Processes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
