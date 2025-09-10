using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarnManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFarmEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Farms_Users_UserId",
                table: "Farms");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Farms",
                newName: "LastUpdatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Farms",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Farms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Farms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Farms_Users_UserId",
                table: "Farms",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Farms_Users_UserId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Farms");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "Farms",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Farms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Farms_Users_UserId",
                table: "Farms",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
