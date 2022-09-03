using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class UserIdUniqueRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verifications_UserId",
                table: "Verifications");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 232, 233, 250, 107, 192, 107, 205, 224, 55, 103, 155, 57, 92, 64, 57, 239, 162, 78, 37, 174, 10, 63, 113, 35, 136, 80, 137, 168, 27, 105, 18, 31, 0, 36, 234, 87, 180, 79, 62, 130, 19, 203, 83, 144, 96, 81, 29, 126, 174, 19, 52, 53, 133, 63, 197, 249, 221, 173, 64, 227, 107, 11, 253, 56 }, new byte[] { 192, 128, 137, 187, 41, 56, 156, 69, 238, 124, 86, 103, 93, 118, 181, 71, 172, 221, 72, 54, 98, 167, 192, 174, 182, 128, 64, 237, 31, 130, 186, 136, 141, 187, 202, 152, 188, 98, 81, 154, 142, 173, 7, 13, 139, 19, 21, 210, 134, 238, 174, 150, 17, 77, 208, 119, 166, 117, 119, 175, 40, 133, 12, 251, 8, 240, 187, 154, 140, 149, 79, 202, 189, 80, 215, 233, 90, 92, 213, 220, 78, 101, 124, 140, 10, 9, 160, 117, 10, 6, 138, 214, 131, 84, 62, 13, 143, 237, 47, 190, 34, 209, 235, 102, 24, 204, 188, 78, 67, 239, 85, 249, 160, 50, 49, 189, 99, 162, 0, 9, 83, 234, 67, 91, 15, 122, 144, 117 } });

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_UserId",
                table: "Verifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verifications_UserId",
                table: "Verifications");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 186, 98, 134, 94, 215, 47, 161, 176, 46, 46, 88, 8, 18, 237, 159, 0, 123, 0, 70, 214, 246, 216, 9, 31, 44, 162, 83, 86, 31, 92, 63, 118, 112, 226, 72, 86, 140, 62, 252, 111, 224, 186, 81, 74, 104, 247, 199, 127, 123, 202, 48, 125, 8, 177, 219, 212, 106, 240, 77, 5, 47, 252, 206, 138 }, new byte[] { 187, 167, 96, 139, 15, 190, 224, 215, 11, 230, 48, 142, 28, 33, 106, 51, 235, 3, 225, 159, 149, 180, 155, 182, 87, 128, 151, 102, 54, 222, 99, 242, 248, 220, 212, 16, 94, 247, 110, 125, 33, 18, 131, 159, 86, 52, 30, 132, 249, 36, 102, 226, 224, 174, 148, 155, 11, 116, 229, 254, 78, 237, 118, 95, 101, 54, 69, 192, 216, 239, 226, 96, 40, 133, 248, 115, 230, 40, 102, 199, 147, 80, 224, 184, 235, 67, 9, 145, 244, 108, 114, 90, 207, 27, 133, 128, 145, 89, 86, 8, 227, 125, 229, 178, 223, 222, 191, 161, 153, 153, 239, 120, 127, 55, 173, 128, 151, 48, 254, 85, 43, 221, 149, 8, 20, 222, 13, 67 } });

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_UserId",
                table: "Verifications",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets",
                column: "UserId",
                unique: true);
        }
    }
}
