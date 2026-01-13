using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminWebPage.Migrations
{
    /// <inheritdoc />
    public partial class fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Account",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
