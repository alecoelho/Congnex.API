using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "level",
                table: "lessons",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "lessons");
        }
    }
}
