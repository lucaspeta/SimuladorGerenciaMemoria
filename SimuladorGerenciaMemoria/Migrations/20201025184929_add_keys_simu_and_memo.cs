using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class add_keys_simu_and_memo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Simulations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SimulationID",
                table: "Memories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Simulations_UserID",
                table: "Simulations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_SimulationID",
                table: "Memories",
                column: "SimulationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories",
                column: "SimulationID",
                principalTable: "Simulations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Simulations_Users_UserID",
                table: "Simulations",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Simulations_SimulationID",
                table: "Memories");

            migrationBuilder.DropForeignKey(
                name: "FK_Simulations_Users_UserID",
                table: "Simulations");

            migrationBuilder.DropIndex(
                name: "IX_Simulations_UserID",
                table: "Simulations");

            migrationBuilder.DropIndex(
                name: "IX_Memories_SimulationID",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Simulations");

            migrationBuilder.DropColumn(
                name: "SimulationID",
                table: "Memories");
        }
    }
}
