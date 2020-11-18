using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class process_inseted_memory_simulation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BestFitInseridos",
                table: "Memories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FirstFitInseridos",
                table: "Memories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextFitInseridos",
                table: "Memories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorstFitInseridos",
                table: "Memories",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestFitInseridos",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "FirstFitInseridos",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "NextFitInseridos",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "WorstFitInseridos",
                table: "Memories");
        }
    }
}
