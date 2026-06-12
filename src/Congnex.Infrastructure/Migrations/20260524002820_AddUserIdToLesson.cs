using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add user_id column first
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "lessons",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            // Create a temporary standalone index on unit_id so the FK has support
            // while we drop and recreate the composite index
            migrationBuilder.CreateIndex(
                name: "IX_lessons_unit_id_temp",
                table: "lessons",
                column: "unit_id");

            // Drop the FK that was backed by the composite unique index
            migrationBuilder.DropForeignKey(
                name: "FK_lessons_units_unit_id",
                table: "lessons");

            // Now safe to drop the composite unique index
            migrationBuilder.DropIndex(
                name: "IX_lessons_unit_id_order_index",
                table: "lessons");

            // Recreate as non-unique (multiple users may share same order_index in a unit)
            migrationBuilder.CreateIndex(
                name: "IX_lessons_unit_id_order_index",
                table: "lessons",
                columns: new[] { "unit_id", "order_index" });

            // Re-add the unit FK
            migrationBuilder.AddForeignKey(
                name: "FK_lessons_units_unit_id",
                table: "lessons",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // Drop temporary index — composite index now satisfies the FK
            migrationBuilder.DropIndex(
                name: "IX_lessons_unit_id_temp",
                table: "lessons");

            // Add user_id index and FK
            migrationBuilder.CreateIndex(
                name: "IX_lessons_user_id",
                table: "lessons",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_lessons_users_user_id",
                table: "lessons",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lessons_users_user_id",
                table: "lessons");

            // Create temp index so unit FK has support during reversal
            migrationBuilder.CreateIndex(
                name: "IX_lessons_unit_id_temp",
                table: "lessons",
                column: "unit_id");

            migrationBuilder.DropForeignKey(
                name: "FK_lessons_units_unit_id",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "IX_lessons_unit_id_order_index",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "IX_lessons_user_id",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "lessons");

            // Restore original unique composite index
            migrationBuilder.CreateIndex(
                name: "IX_lessons_unit_id_order_index",
                table: "lessons",
                columns: new[] { "unit_id", "order_index" },
                unique: true);

            // Re-add unit FK backed by the restored unique index
            migrationBuilder.AddForeignKey(
                name: "FK_lessons_units_unit_id",
                table: "lessons",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropIndex(
                name: "IX_lessons_unit_id_temp",
                table: "lessons");
        }
    }
}
