using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsAndSubscriptionDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_device_tokens_users_UserId1",
                table: "device_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_preferences_users_UserId1",
                table: "notification_preferences");

            migrationBuilder.DropForeignKey(
                name: "FK_study_plans_users_UserId1",
                table: "study_plans");

            migrationBuilder.DropIndex(
                name: "IX_study_plans_UserId1",
                table: "study_plans");

            migrationBuilder.DropIndex(
                name: "IX_notification_preferences_UserId1",
                table: "notification_preferences");

            migrationBuilder.DropIndex(
                name: "IX_device_tokens_UserId1",
                table: "device_tokens");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "study_plans");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "device_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "subscriptions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldDefaultValue: "Active")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_user_id",
                table: "study_plans",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_study_plans_user_id",
                table: "study_plans");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "subscriptions",
                type: "longtext",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "study_plans",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "notification_preferences",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "device_tokens",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_UserId1",
                table: "study_plans",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_UserId1",
                table: "notification_preferences",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_tokens_UserId1",
                table: "device_tokens",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_device_tokens_users_UserId1",
                table: "device_tokens",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_notification_preferences_users_UserId1",
                table: "notification_preferences",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_study_plans_users_UserId1",
                table: "study_plans",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
