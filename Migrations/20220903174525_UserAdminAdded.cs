using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class UserAdminAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt", "PictureURL", "Role", "Username", "Verified" },
                values: new object[] { -1, "admin@admin.com", "Admin", "Admin", new byte[] { 242, 40, 5, 171, 200, 50, 115, 77, 20, 91, 227, 24, 21, 236, 162, 142, 100, 6, 73, 161, 56, 152, 68, 2, 57, 48, 150, 197, 22, 82, 107, 26, 134, 149, 222, 178, 181, 41, 204, 166, 191, 92, 96, 143, 220, 56, 94, 11, 251, 215, 235, 104, 34, 132, 103, 234, 231, 153, 162, 170, 180, 119, 173, 236 }, new byte[] { 177, 119, 215, 188, 118, 173, 141, 138, 49, 247, 63, 88, 176, 227, 10, 6, 23, 175, 41, 150, 228, 104, 244, 194, 180, 36, 225, 215, 252, 253, 56, 15, 33, 63, 248, 11, 80, 231, 173, 123, 81, 84, 29, 29, 50, 159, 172, 204, 40, 148, 207, 160, 168, 180, 142, 164, 178, 64, 16, 193, 144, 38, 142, 215, 205, 41, 83, 128, 137, 230, 139, 167, 90, 216, 168, 139, 3, 211, 145, 105, 160, 31, 24, 91, 99, 81, 4, 220, 66, 141, 107, 94, 186, 225, 77, 153, 75, 38, 241, 113, 140, 169, 135, 27, 118, 48, 93, 85, 221, 42, 58, 37, 236, 119, 244, 224, 144, 201, 88, 171, 139, 200, 2, 17, 46, 55, 222, 35 }, null, "Admin", "admin", true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
