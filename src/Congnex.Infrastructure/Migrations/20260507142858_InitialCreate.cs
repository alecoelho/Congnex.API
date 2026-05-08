using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    google_sub = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apple_sub = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    xp = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    streak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    lives = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    energy = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    max_energy = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    last_lesson_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    last_life_regen_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    plan = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Free")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    daily_minutes = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "en")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivations = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    refresh_token_hash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    refresh_token_expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    unit_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    xp_reward = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.id);
                    table.ForeignKey(
                        name: "FK_lessons_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ai_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    prompt = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    options = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    correct_index = table.Column<int>(type: "int", nullable: false),
                    topic = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_questions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    token = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hub_registration_id = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_tokens_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_device_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    frequency_hours = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    start_hour = table.Column<int>(type: "int", nullable: false, defaultValue: 8),
                    end_hour = table.Column<int>(type: "int", nullable: false, defaultValue: 20),
                    content_types = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "study_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    assessed_level = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    generated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_plans_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_study_plans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    stripe_customer_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stripe_subscription_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stripe_price_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cancel_at_period_end = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    cancel_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    current_period_start = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    current_period_end = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    lesson_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prompt = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    correct_answers = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    options = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    media_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    lesson_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_progress_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_progress_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "review_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ai_question_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    source = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stability = table.Column<float>(type: "float", nullable: false),
                    difficulty = table.Column<float>(type: "float", nullable: false, defaultValue: 5f),
                    due_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_review_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    reps = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    lapses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    state = table.Column<string>(type: "longtext", nullable: false, defaultValue: "New")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_items_ai_questions_ai_question_id",
                        column: x => x.ai_question_id,
                        principalTable: "ai_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_review_items_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_review_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    given_answers = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_correct = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    answered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_answers_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_answers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ai_questions_user_id_is_active",
                table: "ai_questions",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_device_tokens_token",
                table: "device_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_tokens_user_id",
                table: "device_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_tokens_UserId1",
                table: "device_tokens",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_lessons_unit_id_order_index",
                table: "lessons",
                columns: new[] { "unit_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_user_id",
                table: "notification_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_UserId1",
                table: "notification_preferences",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_questions_lesson_id_order_index",
                table: "questions",
                columns: new[] { "lesson_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_review_items_ai_question_id",
                table: "review_items",
                column: "ai_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_items_question_id",
                table: "review_items",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_items_user_id_due_date",
                table: "review_items",
                columns: new[] { "user_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_user_id_generated_at",
                table: "study_plans",
                columns: new[] { "user_id", "generated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_UserId1",
                table: "study_plans",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_stripe_customer_id",
                table: "subscriptions",
                column: "stripe_customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_stripe_subscription_id",
                table: "subscriptions",
                column: "stripe_subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_units_language_code_order_index",
                table: "units",
                columns: new[] { "language_code", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_question_id",
                table: "user_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_user_id_answered_at",
                table: "user_answers",
                columns: new[] { "user_id", "answered_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_UserId1",
                table: "user_answers",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_progress_lesson_id",
                table: "user_progress",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_progress_user_id_lesson_id",
                table: "user_progress",
                columns: new[] { "user_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_progress_UserId1",
                table: "user_progress",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_users_apple_sub",
                table: "users",
                column: "apple_sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_google_sub",
                table: "users",
                column: "google_sub",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_tokens");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "review_items");

            migrationBuilder.DropTable(
                name: "study_plans");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "user_answers");

            migrationBuilder.DropTable(
                name: "user_progress");

            migrationBuilder.DropTable(
                name: "ai_questions");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "units");
        }
    }
}
