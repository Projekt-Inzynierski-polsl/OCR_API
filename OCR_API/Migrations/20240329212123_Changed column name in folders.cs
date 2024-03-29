using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR_API.Migrations
{
    /// <inheritdoc />
    public partial class Changedcolumnnameinfolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "hashed_password",
                table: "folders",
                newName: "password_hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "folders",
                newName: "hashed_password");
        }
    }
}
