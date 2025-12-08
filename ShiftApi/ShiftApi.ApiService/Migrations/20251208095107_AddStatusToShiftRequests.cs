using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftApi.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToShiftRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "ShiftRequests",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ShiftRequests");
        }
    }
}
