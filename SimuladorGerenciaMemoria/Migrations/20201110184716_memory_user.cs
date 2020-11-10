using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class memory_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Memories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memories_UserID",
                table: "Memories",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Users_UserID",
                table: "Memories",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Users_UserID",
                table: "Memories");

            migrationBuilder.DropIndex(
                name: "IX_Memories_UserID",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Memories");
        }
    }
}
