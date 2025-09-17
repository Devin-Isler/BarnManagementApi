using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BarnManagementApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BarnManagementApi.Tests
{
	public class ProductTests
	{
		private static DbContextOptions<BarnDbContext> GetInMemoryOptions()
		{
			return new DbContextOptionsBuilder<BarnDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
		}

		private static IMapper CreateMapper()
		{
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new Mapping.MappingProfile());
			});
			return mapperConfig.CreateMapper();
		}

		private static (Guid userId, Guid farmId) SeedUserAndFarm(BarnDbContext context)
		{
			var userId = Guid.NewGuid();
			var farmId = Guid.NewGuid();

			var user = new User
			{
				Id = userId,
				Username = "user@example.com",
				PasswordHash = "hash",
				Balance = 1000m,
				CreatedAt = DateTime.UtcNow
			};

			var farm = new Farm
			{
				Id = farmId,
				UserId = userId,
				Name = "Ana Çiftlik",
				Description = "Açıklama",
				Location = "Bursa"
			};

			context.Users.Add(user);
			context.Farms.Add(farm);
			context.SaveChanges();

			return (userId, farmId);
		}

		private static AnimalType SeedAnimalType(BarnDbContext context, string name = "Chicken")
		{
			var type = new AnimalType
			{
				Id = Guid.NewGuid(),
				Name = name,
				Lifetime = 60,
				ProductionInterval = 10,
				PurchasePrice = 50m,
				DefaultSellPrice = 70m,
				ProducedProductName = name + " Product",
				ProducedProductSellPrice = 2m
			};
			context.AnimalType.Add(type);
			context.SaveChanges();
			return type;
		}

		private static ProductType SeedProductType(BarnDbContext context, string name, decimal price = 2m)
		{
			var pt = new ProductType
			{
				Id = Guid.NewGuid(),
				Name = name,
				DefaultSellPrice = price
			};
			context.ProductTypes.Add(pt);
			context.SaveChanges();
			return pt;
		}

		private static Animal CreateAnimal(Guid farmId, Guid animalTypeId, string name = "Tavuk")
		{
			return new Animal
			{
				Id = Guid.NewGuid(),
				Name = name,
				Lifetime = 60,
				ProductionInterval = 10,
				PurchasePrice = 50m,
				SellPrice = 70m,
				FarmId = farmId,
				AnimalTypeId = animalTypeId,
				CreatedAt = DateTime.UtcNow.AddMinutes(-20),
				IsActive = true,
				LastProductionTime = DateTime.UtcNow.AddMinutes(-11)
			};
		}

		[Fact]
		public async Task GetAll_Should_Filter_DefaultUnsold_Sort_Page()
		{
			var options = GetInMemoryOptions();

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (userId, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context, name: "Hen");
				var pType = SeedProductType(context, name: "Hen Product", price: 5m);

				var a1 = CreateAnimal(farmId, aType.Id, name: "A1");
				var a2 = CreateAnimal(farmId, aType.Id, name: "A2");
				context.Animals.AddRange(a1, a2);
				await context.SaveChangesAsync();

				// Seed products
				context.Products.AddRange(
					new Product { Id = Guid.NewGuid(), AnimalId = a1.Id, Name = pType.Name, Price = 5m, CreatedAt = DateTime.UtcNow.AddMinutes(-3), IsSold = false },
					new Product { Id = Guid.NewGuid(), AnimalId = a2.Id, Name = "Z-Other", Price = 9m, CreatedAt = DateTime.UtcNow.AddMinutes(-2), IsSold = true },
					new Product { Id = Guid.NewGuid(), AnimalId = a1.Id, Name = "B-Other", Price = 3m, CreatedAt = DateTime.UtcNow.AddMinutes(-1), IsSold = false }
				);
				await context.SaveChangesAsync();
			}

			// Act & Assert
			await using (var context = new BarnDbContext(options))
			{
				var userId = context.Farms.First().UserId;
				var repo = new SQLProductRepository(context);

				var page1 = await repo.GetAllProductsAsync(userId, sortBy: "CreatedAt", isAscending: true, pageNumber: 1, pageSize: 1);
				Assert.Single(page1);
				Assert.Equal("Hen Product", page1[0].Name);

				var page2 = await repo.GetAllProductsAsync(userId, sortBy: "CreatedAt", isAscending: true, pageNumber: 2, pageSize: 1);
				Assert.Single(page2);
				Assert.Equal("B-Other", page2[0].Name);

				var filteredSold = await repo.GetAllProductsAsync(userId, filterOn: "IsSold", filterQuery: "true");
				Assert.Single(filteredSold);
				Assert.Equal("Z-Other", filteredSold[0].Name);

				var byName = await repo.GetAllProductsAsync(userId, filterOn: "Name", filterQuery: "Hen");
				Assert.Single(byName);
				Assert.Equal("Hen Product", byName[0].Name);
			}
		}

		[Fact]
		public async Task GetAll_Should_Filter_ByFarm_And_ByAnimal()
		{
			var options = GetInMemoryOptions();

			Guid farmA;
			Guid farmB;
			Guid animalA1;

			await using (var context = new BarnDbContext(options))
			{
				var (_, f1) = SeedUserAndFarm(context);
				var (_, f2) = SeedUserAndFarm(context);
				farmA = f1;
				farmB = f2;

				var aType = SeedAnimalType(context, name: "Cow");
				var pType = SeedProductType(context, name: "Cow Product", price: 7m);

				var a1 = CreateAnimal(farmA, aType.Id, name: "A1");
				var a2 = CreateAnimal(farmB, aType.Id, name: "A2");
				context.Animals.AddRange(a1, a2);
				await context.SaveChangesAsync();
				animalA1 = a1.Id;

				context.Products.AddRange(
					new Product { Id = Guid.NewGuid(), AnimalId = a1.Id, Name = pType.Name, Price = 7m, CreatedAt = DateTime.UtcNow, IsSold = false },
					new Product { Id = Guid.NewGuid(), AnimalId = a2.Id, Name = pType.Name, Price = 7m, CreatedAt = DateTime.UtcNow, IsSold = false }
				);
				await context.SaveChangesAsync();
			}

			await using (var context = new BarnDbContext(options))
			{
				var userA = context.Farms.OrderBy(f => f.CreatedAt).First().UserId;
				var userB = context.Farms.OrderBy(f => f.CreatedAt).Skip(1).First().UserId;
				var repo = new SQLProductRepository(context);

				var onlyFarmA = await repo.GetAllProductsAsync(userA, filterOn: "FarmId", filterQuery: farmA.ToString());
				Assert.All(onlyFarmA, p => Assert.Equal(farmA, p.Animal.FarmId));

				var onlyByAnimal = await repo.GetAllProductsAsync(userA, filterOn: "AnimalId", filterQuery: animalA1.ToString());
				Assert.All(onlyByAnimal, p => Assert.Equal(animalA1, p.AnimalId));

				var noneForUserBWhenFilteringFarmA = await repo.GetAllProductsAsync(userB, filterOn: "FarmId", filterQuery: farmA.ToString());
				Assert.Empty(noneForUserBWhenFilteringFarmA);
			}
		}

		[Fact]
		public async Task GetById_Should_ReturnOnly_Unsold()
		{
			var options = GetInMemoryOptions();
			Guid unsoldId;
			Guid soldId;

			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context);
				var pType = SeedProductType(context, name: "Chicken Product");
				var animal = CreateAnimal(farmId, aType.Id);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();

				var p1 = new Product { Id = Guid.NewGuid(), AnimalId = animal.Id, Name = pType.Name, Price = 2m, IsSold = false };
				var p2 = new Product { Id = Guid.NewGuid(), AnimalId = animal.Id, Name = pType.Name, Price = 2m, IsSold = true, SoldAt = DateTime.UtcNow };
				context.Products.AddRange(p1, p2);
				await context.SaveChangesAsync();
				unsoldId = p1.Id;
				soldId = p2.Id;
			}

			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLProductRepository(context);
				var foundUnsold = await repo.GetProductByIdAsync(unsoldId);
				var foundSold = await repo.GetProductByIdAsync(soldId);
				Assert.NotNull(foundUnsold);
				Assert.Null(foundSold);
			}
		}

		[Fact]
		public async Task Produce_Should_CreateProduct_FromAnimalType_And_ProductType()
		{
			var options = GetInMemoryOptions();

			Guid animalId;
			decimal defaultPrice;

			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context, name: "Sheep");
				var pType = SeedProductType(context, name: "Sheep Product", price: 4m);
				defaultPrice = pType.DefaultSellPrice;
				var animal = CreateAnimal(farmId, aType.Id, name: "Sh1");
				context.Animals.Add(animal);
				await context.SaveChangesAsync();
				animalId = animal.Id;
			}

			await using (var context = new BarnDbContext(options))
			{
				var mapper = CreateMapper();
				var repo = new SQLProductRepository(context);
				var created = await repo.ProduceProductAsync(animalId);
				Assert.NotNull(created);

				var dto = mapper.Map<ProductDto>(created);
				Assert.Equal("Sheep Product", dto.Name);
				Assert.Equal(defaultPrice, dto.Price);
				Assert.False(dto.IsSold);
			}
		}

		[Fact]
		public async Task Produce_Should_Throw_When_AnimalMissing_AnimalTypeMissing_Or_ProductTypeMissing()
		{
			var options = GetInMemoryOptions();

			// Missing animal
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLProductRepository(context);
				await Assert.ThrowsAsync<Exception>(async () => await repo.ProduceProductAsync(Guid.NewGuid()));
			}

			// Missing product type
			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context, name: "Goat"); // produces "Goat Product"
				var animal = CreateAnimal(farmId, aType.Id);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();

				var repo = new SQLProductRepository(context);
				await Assert.ThrowsAsync<Exception>(async () => await repo.ProduceProductAsync(animal.Id));
			}
		}

		[Fact]
		public async Task Update_Should_ChangeName_And_SellPrice()
		{
			var options = GetInMemoryOptions();
			Guid productId;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context);
				var pType = SeedProductType(context, name: "Chicken Product", price: 2m);
				var animal = CreateAnimal(farmId, aType.Id);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();

				var product = new Product { Id = Guid.NewGuid(), AnimalId = animal.Id, Name = pType.Name, Price = 2m, CreatedAt = DateTime.UtcNow };
				context.Products.Add(product);
				await context.SaveChangesAsync();
				productId = product.Id;
			}

			// Act
			await using (var context = new BarnDbContext(options))
			{
				var mapper = CreateMapper();
				var repo = new SQLProductRepository(context);
				var updateDto = new ProductUpdateDto { Name = "Egg", Price = 3.5m };
				var update = mapper.Map<Product>(updateDto);
				update.Name ??= string.Empty;
				update.Price = updateDto.Price ?? 0m;
				var updated = await repo.UpdateProductAsync(productId, update);

				// Assert
				Assert.NotNull(updated);
				Assert.Equal("Egg", updated!.Name);
				Assert.Equal(3.5m, updated.Price);
			}
		}

		[Fact]
		public async Task Sell_Should_SetSoldAt_And_AddBalance()
		{
			var options = GetInMemoryOptions();
			Guid productId;
			decimal startBalance;

			await using (var context = new BarnDbContext(options))
			{
				var (userId, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context);
				var pType = SeedProductType(context, name: "Chicken Product", price: 4m);
				var animal = CreateAnimal(farmId, aType.Id);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();

				var product = new Product { Id = Guid.NewGuid(), AnimalId = animal.Id, Name = pType.Name, Price = 4m, CreatedAt = DateTime.UtcNow, IsSold = false };
				context.Products.Add(product);
				await context.SaveChangesAsync();
				productId = product.Id;
				startBalance = context.Users.First(u => u.Id == userId).Balance;
			}

			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLProductRepository(context);
				var sold = await repo.SellProductAsync(productId);
				Assert.NotNull(sold);
				Assert.True(sold!.IsSold);
				Assert.NotNull(sold.SoldAt);
			}

			await using (var context = new BarnDbContext(options))
			{
				var user = context.Users.First();
				Assert.Equal(startBalance + 4m, user.Balance);
			}
		}

		[Fact]
		public async Task Delete_Should_RemoveProduct()
		{
			var options = GetInMemoryOptions();
			Guid productId;

			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var aType = SeedAnimalType(context);
				var pType = SeedProductType(context, name: "Chicken Product");
				var animal = CreateAnimal(farmId, aType.Id);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();

				var product = new Product { Id = Guid.NewGuid(), AnimalId = animal.Id, Name = pType.Name, Price = 2m };
				context.Products.Add(product);
				await context.SaveChangesAsync();
				productId = product.Id;
			}

			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLProductRepository(context);
				var deleted = await repo.DeleteProductAsync(productId);
				Assert.NotNull(deleted);
			}

			await using (var context = new BarnDbContext(options))
			{
				Assert.False(context.Products.Any(p => p.Id == productId));
			}
		}

		[Fact]
		public async Task ProductService_Function_Should_Produce_Only_For_Due_Animal()
		{
			// Arrange: fixed InMemory db to share state across scopes
			var services = new ServiceCollection();
			services.AddLogging();
			var dbName = Guid.NewGuid().ToString();
			services.AddDbContext<BarnDbContext>(opt => opt.UseInMemoryDatabase(dbName));
			services.AddScoped<IProductRepository, SQLProductRepository>();

			var provider = services.BuildServiceProvider();

			Guid dueAnimalId;
			Guid notDueAnimalId;
            await using (var scope = provider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
                var (_, farmId) = SeedUserAndFarm(db);

                var aType = SeedAnimalType(db, name: "Hen");
                var _ = SeedProductType(db, name: "Hen Product", price: 5m);

                var now = DateTime.UtcNow;

                var dueAnimal = CreateAnimal(farmId, aType.Id, name: "Hen-Due");
                // Ensure due
                dueAnimal.LastProductionTime = now.AddMinutes(-11);

                var notDueAnimal = CreateAnimal(farmId, aType.Id, name: "Hen-NotDue");
                // Make not due
                notDueAnimal.LastProductionTime = now.AddMinutes(-3);
                notDueAnimal.CreatedAt = now.AddMinutes(-5);

                db.Animals.AddRange(dueAnimal, notDueAnimal);
                await db.SaveChangesAsync();
                dueAnimalId = dueAnimal.Id;
                notDueAnimalId = notDueAnimal.Id;
            }

            // Act
            var service = new ProductService(provider);
            await service.ProduceDueProductsAsync(System.Threading.CancellationToken.None);

			// Assert: exactly one product created for the due animal
			await using (var scope = provider.CreateAsyncScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
				var products = await db.Products.ToListAsync();
				Assert.Single(products);
				var product = products[0];
				Assert.Equal(dueAnimalId, product.AnimalId);
				Assert.Equal("Hen Product", product.Name);
			}
		}
	}
}
