using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class EmailVerificationAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Valid",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valid",
                table: "Users");
        }
    }
}
