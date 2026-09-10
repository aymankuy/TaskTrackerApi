using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskItemCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatedByUserId",
                table: "TaskItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_CreatedByUserId",
                table: "TaskItems",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_CreatedByUserId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatedByUserId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TaskItems");
        }
    }
}
