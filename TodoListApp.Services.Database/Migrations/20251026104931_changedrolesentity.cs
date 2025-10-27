using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.Services.Database.Migrations
{
    public partial class changedrolesentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint that's causing issues
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListUserRoles_User_UserId",
                table: "TodoListUserRoles");

            // Drop the index too
            migrationBuilder.DropIndex(
                name: "IX_TodoListUserRoles_UserId",
                table: "TodoListUserRoles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate if needed for rollback
            migrationBuilder.CreateIndex(
                name: "IX_TodoListUserRoles_UserId",
                table: "TodoListUserRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListUserRoles_User_UserId",
                table: "TodoListUserRoles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
