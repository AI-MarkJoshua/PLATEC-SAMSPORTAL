using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminWebPage.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIDAndAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherID",
                table: "Account",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "Account");
        }
    }
}
