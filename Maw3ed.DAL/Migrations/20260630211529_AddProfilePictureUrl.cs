using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maw3ed.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-id-1",
                column: "ProfilePictureUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-id-2",
                column: "ProfilePictureUrl",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");
        }
    }
}
