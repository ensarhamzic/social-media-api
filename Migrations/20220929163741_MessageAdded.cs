using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class MessageAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeSent = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 76, 135, 58, 71, 102, 104, 216, 215, 53, 108, 107, 8, 93, 100, 223, 190, 250, 202, 70, 255, 192, 246, 175, 171, 211, 191, 210, 185, 199, 167, 95, 66, 219, 147, 56, 169, 196, 70, 208, 69, 201, 179, 12, 133, 55, 60, 112, 47, 215, 35, 79, 139, 66, 5, 234, 165, 216, 179, 139, 150, 150, 28, 98, 105 }, new byte[] { 211, 175, 48, 106, 28, 157, 14, 61, 23, 58, 140, 62, 101, 114, 255, 173, 61, 230, 24, 203, 190, 198, 234, 151, 38, 253, 217, 225, 219, 31, 207, 47, 139, 91, 78, 213, 74, 188, 53, 26, 50, 204, 213, 26, 99, 248, 29, 154, 192, 28, 35, 126, 167, 50, 127, 187, 35, 188, 156, 82, 165, 205, 121, 19, 80, 54, 24, 209, 179, 61, 57, 21, 85, 195, 134, 35, 210, 38, 92, 176, 84, 24, 176, 52, 134, 161, 67, 3, 141, 174, 156, 13, 52, 15, 98, 17, 23, 199, 18, 50, 24, 113, 186, 101, 61, 160, 173, 44, 245, 149, 251, 140, 245, 234, 189, 63, 205, 84, 79, 232, 89, 91, 166, 1, 78, 77, 34, 187 } });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_FromUserId",
                table: "Messages",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ToUserId",
                table: "Messages",
                column: "ToUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 232, 233, 250, 107, 192, 107, 205, 224, 55, 103, 155, 57, 92, 64, 57, 239, 162, 78, 37, 174, 10, 63, 113, 35, 136, 80, 137, 168, 27, 105, 18, 31, 0, 36, 234, 87, 180, 79, 62, 130, 19, 203, 83, 144, 96, 81, 29, 126, 174, 19, 52, 53, 133, 63, 197, 249, 221, 173, 64, 227, 107, 11, 253, 56 }, new byte[] { 192, 128, 137, 187, 41, 56, 156, 69, 238, 124, 86, 103, 93, 118, 181, 71, 172, 221, 72, 54, 98, 167, 192, 174, 182, 128, 64, 237, 31, 130, 186, 136, 141, 187, 202, 152, 188, 98, 81, 154, 142, 173, 7, 13, 139, 19, 21, 210, 134, 238, 174, 150, 17, 77, 208, 119, 166, 117, 119, 175, 40, 133, 12, 251, 8, 240, 187, 154, 140, 149, 79, 202, 189, 80, 215, 233, 90, 92, 213, 220, 78, 101, 124, 140, 10, 9, 160, 117, 10, 6, 138, 214, 131, 84, 62, 13, 143, 237, 47, 190, 34, 209, 235, 102, 24, 204, 188, 78, 67, 239, 85, 249, 160, 50, 49, 189, 99, 162, 0, 9, 83, 234, 67, 91, 15, 122, 144, 117 } });
        }
    }
}
