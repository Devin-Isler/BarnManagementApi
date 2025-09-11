using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarnManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsAliveToIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAlive",
                table: "Animals",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Animals",
                newName: "IsAlive");
        }
    }
}
