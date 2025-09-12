using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarnManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreAnimalTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sheepId = "33333333-3333-3333-3333-333333333333";
            var goatId = "44444444-4444-4444-4444-444444444444";
            var duckId = "55555555-5555-5555-5555-555555555555";

            migrationBuilder.Sql($@"
                IF NOT EXISTS (SELECT 1 FROM AnimalType WHERE Name = 'Sheep')
                INSERT INTO AnimalType (Id, Name, Lifetime, ProductionInterval, PurchasePrice, DefaultSellPrice, ProducedProductName, ProducedProductSellPrice)
                VALUES ('{sheepId}', 'Sheep', {60 * 24 * 7}, {60 * 24}, 60.0, 50.0, 'Wool', 4.0);

                IF NOT EXISTS (SELECT 1 FROM AnimalType WHERE Name = 'Goat')
                INSERT INTO AnimalType (Id, Name, Lifetime, ProductionInterval, PurchasePrice, DefaultSellPrice, ProducedProductName, ProducedProductSellPrice)
                VALUES ('{goatId}', 'Goat', {60 * 24 * 8}, {60 * 12}, 70.0, 60.0, 'Goat Milk', 2.0);

                IF NOT EXISTS (SELECT 1 FROM AnimalType WHERE Name = 'Duck')
                INSERT INTO AnimalType (Id, Name, Lifetime, ProductionInterval, PurchasePrice, DefaultSellPrice, ProducedProductName, ProducedProductSellPrice)
                VALUES ('{duckId}', 'Duck', {60 * 24 * 3}, {60 * 8}, 15.0, 12.0, 'Duck Egg', 2.0);

                -- Backfill Animals by name to new types
                UPDATE a SET a.AnimalTypeId = t.Id
                FROM Animals a
                INNER JOIN AnimalType t ON a.Name = t.Name
                WHERE a.AnimalTypeId IS NOT NULL; -- keep already-mapped animals as-is
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM AnimalType WHERE Name IN ('Sheep','Goat','Duck');
            ");
        }
    }
}
