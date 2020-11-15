using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class booleans_alg_memoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBestFitCompleted",
                table: "Memories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstFitCompleted",
                table: "Memories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNextFitCompleted",
                table: "Memories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWorstFitCompleted",
                table: "Memories",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBestFitCompleted",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "IsFirstFitCompleted",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "IsNextFitCompleted",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "IsWorstFitCompleted",
                table: "Memories");
        }
    }
}
