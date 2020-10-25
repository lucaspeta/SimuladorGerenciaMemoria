using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class add_keys_simu_and_memo2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories");

            migrationBuilder.AlterColumn<int>(
                name: "SimulationID",
                table: "Memories",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories",
                column: "SimulationID",
                principalTable: "Simulations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories");

            migrationBuilder.AlterColumn<int>(
                name: "SimulationID",
                table: "Memories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories",
                column: "SimulationID",
                principalTable: "Simulations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
