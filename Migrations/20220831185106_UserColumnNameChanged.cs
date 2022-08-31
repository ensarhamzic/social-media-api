using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class UserColumnNameChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Valid",
                table: "Users",
                newName: "Verified");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Verified",
                table: "Users",
                newName: "Valid");
        }
    }
}
