using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class memory_create_init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isGeneratedProcessList",
                table: "Memories",
                newName: "IsGeneratedProcessList");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsGeneratedProcessList",
                table: "Memories",
                newName: "isGeneratedProcessList");
        }
    }
}
