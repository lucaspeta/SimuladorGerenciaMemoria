using Microsoft.EntityFrameworkCore.Migrations;

namespace SimuladorGerenciaMemoria.Migrations
{
    public partial class modelsupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RegB",
                table: "Processes",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RegL",
                table: "Processes",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "TimeToFindIndex",
                table: "Processes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "FramesQTD",
                table: "Memories",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FramesSize",
                table: "Memories",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "Memories",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Frames",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    ProcessID = table.Column<int>(nullable: true),
                    RegB = table.Column<long>(nullable: false),
                    MemoryID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frames", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Frames_Memories_MemoryID",
                        column: x => x.MemoryID,
                        principalTable: "Memories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Frames_Processes_ProcessID",
                        column: x => x.ProcessID,
                        principalTable: "Processes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Frames_MemoryID",
                table: "Frames",
                column: "MemoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Frames_ProcessID",
                table: "Frames",
                column: "ProcessID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frames");

            migrationBuilder.DropColumn(
                name: "RegB",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "RegL",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "TimeToFindIndex",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "FramesQTD",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "FramesSize",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Memories");
        }
    }
}
