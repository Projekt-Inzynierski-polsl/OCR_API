using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR_API.Migrations
{
    /// <inheritdoc />
    public partial class Hexolo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "object_id",
                table: "user_logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hex_color",
                table: "note_category_list",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "object_id",
                table: "user_logs");

            migrationBuilder.DropColumn(
                name: "hex_color",
                table: "note_category_list");
        }
    }
}
