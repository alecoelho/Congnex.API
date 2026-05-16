using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddXylaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MySQL requires dropping the FK before dropping the index it covers
            migrationBuilder.DropForeignKey(
                name: "FK_questions_lessons_lesson_id",
                table: "questions");

            migrationBuilder.DropIndex(
                name: "IX_questions_lesson_id_order_index",
                table: "questions");

            migrationBuilder.AlterColumn<string>(
                name: "motivations",
                table: "users",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "lessons",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "flashcard_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    lesson_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    word = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remembered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    review_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    correct_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    last_reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    next_review_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    interval_days = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flashcard_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_flashcard_reviews_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flashcard_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "xyla_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cefr_level = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    student_goal = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    student_age = table.Column<int>(type: "int", nullable: true),
                    study_plan_json = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xyla_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_xyla_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "xyla_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    role = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xyla_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_xyla_messages_xyla_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "xyla_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Recreate as UNIQUE (lesson cannot have duplicate order positions)
            migrationBuilder.CreateIndex(
                name: "IX_questions_lesson_id_order_index",
                table: "questions",
                columns: new[] { "lesson_id", "order_index" },
                unique: true);

            // Restore the FK now that its backing index exists again
            migrationBuilder.AddForeignKey(
                name: "FK_questions_lessons_lesson_id",
                table: "questions",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_flashcard_reviews_lesson_id",
                table: "flashcard_reviews",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_flashcard_reviews_user_id_next_review_at",
                table: "flashcard_reviews",
                columns: new[] { "user_id", "next_review_at" });

            migrationBuilder.CreateIndex(
                name: "IX_flashcard_reviews_user_id_word",
                table: "flashcard_reviews",
                columns: new[] { "user_id", "word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_xyla_messages_session_id",
                table: "xyla_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_xyla_sessions_user_id",
                table: "xyla_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_xyla_sessions_user_id_status",
                table: "xyla_sessions",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flashcard_reviews");

            migrationBuilder.DropTable(
                name: "xyla_messages");

            migrationBuilder.DropTable(
                name: "xyla_sessions");

            // MySQL requires dropping the FK before dropping the index it covers
            migrationBuilder.DropForeignKey(
                name: "FK_questions_lessons_lesson_id",
                table: "questions");

            migrationBuilder.DropIndex(
                name: "IX_questions_lesson_id_order_index",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "lessons");

            migrationBuilder.AlterColumn<string>(
                name: "motivations",
                table: "users",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Restore non-unique index and FK
            migrationBuilder.CreateIndex(
                name: "IX_questions_lesson_id_order_index",
                table: "questions",
                columns: new[] { "lesson_id", "order_index" });

            migrationBuilder.AddForeignKey(
                name: "FK_questions_lessons_lesson_id",
                table: "questions",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
