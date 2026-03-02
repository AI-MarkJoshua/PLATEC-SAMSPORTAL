using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminWebPage.Migrations
{
    public partial class AddSectionsAndTeacherAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionID);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSections",
                columns: table => new
                {
                    TeacherSectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSections", x => x.TeacherSectionID);
                    table.ForeignKey(
                        name: "FK_TeacherSections_Accounts_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSections_Sections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "Sections",
                        principalColumn: "SectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "SectionID",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSections_SectionID",
                table: "TeacherSections",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSections_TeacherID",
                table: "TeacherSections",
                column: "TeacherID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Sections_SectionID",
                table: "Accounts",
                column: "SectionID",
                principalTable: "Sections",
                principalColumn: "SectionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Sections_SectionID",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "TeacherSections");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropColumn(
                name: "SectionID",
                table: "Accounts");
        }
    }
}
