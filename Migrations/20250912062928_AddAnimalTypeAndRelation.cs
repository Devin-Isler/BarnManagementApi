using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarnManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalTypeAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create lookup table first
            migrationBuilder.CreateTable(
                name: "AnimalType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lifetime = table.Column<int>(type: "int", nullable: false),
                    ProductionInterval = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultSellPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProducedProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProducedProductSellPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalType", x => x.Id);
                });

            // 2) Create unique index on name
            migrationBuilder.CreateIndex(
                name: "IX_AnimalType_Name",
                table: "AnimalType",
                column: "Name",
                unique: true);

            // 3) Seed common types with deterministic IDs
            var chickenId = new Guid("11111111-1111-1111-1111-111111111111");
            var cowId = new Guid("22222222-2222-2222-2222-222222222222");
            var unknownId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            migrationBuilder.InsertData(
                table: "AnimalType",
                columns: new[] { "Id", "Name", "Lifetime", "ProductionInterval", "PurchasePrice", "DefaultSellPrice", "ProducedProductName", "ProducedProductSellPrice" },
                values: new object[,]
                {
                    { chickenId, "Chicken", 60 * 24 * 2, 60 * 6, 10m, 8m, "Egg", 1m },
                    { cowId, "Cow", 60 * 24 * 10, 60 * 12, 100m, 90m, "Milk", 3m },
                    { unknownId, "Unknown", 60 * 24, 60 * 24, 0m, 0m, "", 0m }
                }
            );

            // 4) Add FK column as nullable to allow backfill
            migrationBuilder.AddColumn<Guid>(
                name: "AnimalTypeId",
                table: "Animals",
                type: "uniqueidentifier",
                nullable: true);

            // 5) Backfill by matching existing Animals.Name to AnimalType.Name
            migrationBuilder.Sql(@"
                UPDATE a
                SET a.AnimalTypeId = t.Id
                FROM Animals a
                INNER JOIN AnimalType t ON a.Name = t.Name;
            ");

            // 6) Any remaining nulls -> Unknown
            migrationBuilder.Sql(@$"
                UPDATE Animals SET AnimalTypeId = '{unknownId}' WHERE AnimalTypeId IS NULL;
            ");

            // 7) Make column non-nullable now that it's populated
            migrationBuilder.AlterColumn<Guid>(
                name: "AnimalTypeId",
                table: "Animals",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // 8) Index and FK
            migrationBuilder.CreateIndex(
                name: "IX_Animals_AnimalTypeId",
                table: "Animals",
                column: "AnimalTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_AnimalType_AnimalTypeId",
                table: "Animals",
                column: "AnimalTypeId",
                principalTable: "AnimalType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_AnimalType_AnimalTypeId",
                table: "Animals");

            migrationBuilder.DropTable(
                name: "AnimalType");

            migrationBuilder.DropIndex(
                name: "IX_Animals_AnimalTypeId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "AnimalTypeId",
                table: "Animals");
        }
    }
}
