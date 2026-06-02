using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Punchly.Migrations
{
    /// <inheritdoc />
    public partial class SplitFullNameIntoFirstAndLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "AppUser",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AppUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AppUser");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AppUser",
                newName: "FullName");
        }
    }
}
