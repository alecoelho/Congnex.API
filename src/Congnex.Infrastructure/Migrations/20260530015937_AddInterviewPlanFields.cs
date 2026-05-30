using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewPlanFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "user_interview_answers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentGoal",
                table: "user_interview_answers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VideoQuery",
                table: "user_interview_answers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VideoTopic",
                table: "user_interview_answers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "end_time",
                table: "lesson_videos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_time",
                table: "lesson_videos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "user_interview_answers");

            migrationBuilder.DropColumn(
                name: "StudentGoal",
                table: "user_interview_answers");

            migrationBuilder.DropColumn(
                name: "VideoQuery",
                table: "user_interview_answers");

            migrationBuilder.DropColumn(
                name: "VideoTopic",
                table: "user_interview_answers");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "lesson_videos");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "lesson_videos");
        }
    }
}
