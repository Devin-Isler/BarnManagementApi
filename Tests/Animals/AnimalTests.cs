using System.Security.Claims;
using BarnManagementApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AutoMapper;

namespace BarnManagementApi.Tests
{
	public class AnimalTests
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

		private static Animal CreateAnimal(Guid farmId, Guid animalTypeId, string name = "Tavuk", decimal purchase = 50m, decimal sell = 70m)
		{
			return new Animal
			{
				Id = Guid.NewGuid(),
				Name = name,
				Lifetime = 60,
				ProductionInterval = 10,
				PurchasePrice = purchase,
				SellPrice = sell,
				FarmId = farmId,
				AnimalTypeId = animalTypeId,
				CreatedAt = DateTime.UtcNow,
				IsActive = true
			};
		}

		[Fact]
		public async Task GetAll_Should_Filter_DefaultActive_Sort_Page()
		{
			var options = GetInMemoryOptions();

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (userId, farmId) = SeedUserAndFarm(context);
				var type = SeedAnimalType(context);

				var a1 = CreateAnimal(farmId, type.Id, name: "Alpha");
				a1.IsActive = true;
				a1.CreatedAt = DateTime.UtcNow.AddMinutes(-3);
				var a2 = CreateAnimal(farmId, type.Id, name: "Beta");
				a2.IsActive = false; // should be excluded by default
				a2.CreatedAt = DateTime.UtcNow.AddMinutes(-2);
				var a3 = CreateAnimal(farmId, type.Id, name: "Gamma");
				a3.IsActive = true;
				a3.CreatedAt = DateTime.UtcNow.AddMinutes(-1);

				context.Animals.AddRange(a1, a2, a3);
				await context.SaveChangesAsync();
			}

			// Act & Assert
			await using (var context = new BarnDbContext(options))
			{
				var (userId, _) = (context.Farms.First().UserId, context.Farms.First().Id);
				var repo = new SQLAnimalRepository(context);

				var page1 = await repo.GetAllAnimalsAsync(userId, sortBy: "CreatedAt", isAscending: true, pageNumber: 1, pageSize: 1);
				// Assert
				Assert.Single(page1);
				Assert.Equal("Alpha", page1[0].Name);

				var page2 = await repo.GetAllAnimalsAsync(userId, sortBy: "CreatedAt", isAscending: true, pageNumber: 2, pageSize: 1);
				// Assert
				Assert.Single(page2);
				Assert.Equal("Gamma", page2[0].Name);

				var filtered = await repo.GetAllAnimalsAsync(userId, filterOn: "IsActive", filterQuery: "false");
				// Assert
				Assert.Single(filtered);
				Assert.Equal("Beta", filtered[0].Name);

				var byName = await repo.GetAllAnimalsAsync(userId, filterOn: "Name", filterQuery: "Ga");
				// Assert
				Assert.Single(byName);
				Assert.Equal("Gamma", byName[0].Name);
			}
		}

		[Fact]
		public async Task GetById_Should_ReturnOnly_Active()
		{
			var options = GetInMemoryOptions();
			Guid activeId;
			Guid inactiveId;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var seed = SeedUserAndFarm(context);
				var type = SeedAnimalType(context);
				var active = CreateAnimal(seed.farmId, type.Id, name: "Active");
				var inactive = CreateAnimal(seed.farmId, type.Id, name: "Inactive");
				inactive.IsActive = false;
				context.Animals.AddRange(active, inactive);
				await context.SaveChangesAsync();
				activeId = active.Id;
				inactiveId = inactive.Id;
			}

			// Act & Assert
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLAnimalRepository(context);
				var foundActive = await repo.GetAnimalByIdAsync(activeId);
				var foundInactive = await repo.GetAnimalByIdAsync(inactiveId);
				// Assert
				Assert.NotNull(foundActive);
				Assert.Null(foundInactive);
			}
		}

		[Fact]
		public async Task BuyByTemplate_Should_CreateAnimal_And_DeductBalance()
		{
			var options = GetInMemoryOptions();
			Guid farmId;
			decimal startBalance;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (_, fId) = SeedUserAndFarm(context);
				farmId = fId;
				SeedAnimalType(context, name: "Cow");
				startBalance = context.Users.First().Balance;
			}

			// Act
			await using (var context = new BarnDbContext(options))
			{
				var mapper = CreateMapper();
				var repo = new SQLAnimalRepository(context);
				var buyDto = new AnimalBuyDto { Name = "Cow", FarmId = farmId };
				var created = await repo.BuyAnimalByTemplateNameAsync(buyDto.Name, buyDto.FarmId);
				// Assert
				Assert.NotNull(created);
				var createdDto = mapper.Map<AnimalDto>(created);
				Assert.True(createdDto.IsActive);
				Assert.Equal("Cow", createdDto.Name);
			}

			// Assert (post-conditions)
			await using (var context = new BarnDbContext(options))
			{
				var user = context.Users.First();
				var template = context.AnimalType.First(t => t.Name == "Cow");
				Assert.Equal(startBalance - template.PurchasePrice, user.Balance);
			}
		}

		[Fact]
		public async Task BuyByTemplate_Should_ReturnNull_When_TemplateMissing_Or_FarmMissing_Or_BudgetLow()
		{
			var options = GetInMemoryOptions();
			Guid farmId;

			// Arrange (low budget)
			await using (var context = new BarnDbContext(options))
			{
				var (userId, fId) = SeedUserAndFarm(context);
				farmId = fId;
				var user = await context.Users.FindAsync(userId);
				user!.Balance = 10m;
				await context.SaveChangesAsync();
			}

			// Act & Assert (missing template)
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLAnimalRepository(context);
				var buyDto = new AnimalBuyDto { Name = "Nope", FarmId = farmId };
				var missingTemplate = await repo.BuyAnimalByTemplateNameAsync(buyDto.Name, buyDto.FarmId);
				Assert.Null(missingTemplate);
			}

			// Arrange (seed template)
			await using (var context = new BarnDbContext(options))
			{
				SeedAnimalType(context, name: "Sheep");
			}

			// Act & Assert (low budget blocks buy)
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLAnimalRepository(context);
				var buyDto = new AnimalBuyDto { Name = "Sheep", FarmId = farmId };
				var lowBudget = await repo.BuyAnimalByTemplateNameAsync(buyDto.Name, buyDto.FarmId);
				Assert.Null(lowBudget);
			}

			// Act & Assert (farm missing)
			await using (var context = new BarnDbContext(options))
			{
				// Remove farm
				context.Farms.RemoveRange(context.Farms);
				await context.SaveChangesAsync();
				var repo = new SQLAnimalRepository(context);
				var buyDto = new AnimalBuyDto { Name = "Sheep", FarmId = Guid.NewGuid() };
				var result = await repo.BuyAnimalByTemplateNameAsync(buyDto.Name, buyDto.FarmId);
				Assert.Null(result);
			}
		}

		[Fact]
		public async Task Update_Should_ChangeName()
		{
			var options = GetInMemoryOptions();
			Guid animalId;
			Guid farmId;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var mapper = CreateMapper();
				var (_, fId) = SeedUserAndFarm(context);
				farmId = fId;
				var type = SeedAnimalType(context);
				var animal = CreateAnimal(farmId, type.Id, name: "Old", purchase: 100m);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();
				animalId = animal.Id;
			}

			// Act
			await using (var context = new BarnDbContext(options))
			{
				var mapper = CreateMapper();
				var repo = new SQLAnimalRepository(context);
				var updateDto = new AnimalUpdateDto { Name = "New" };
				var update = mapper.Map<Animal>(updateDto);
				var updated = await repo.UpdateAnimalAsync(animalId, update);
				// Assert
				Assert.NotNull(updated);
				Assert.Equal("New", updated!.Name);
			}
		}

		[Fact]
		public async Task Sell_Should_SetSoldAt_Inactive_And_AddBalance()
		{
			var options = GetInMemoryOptions();
			Guid animalId;
			decimal startBalance;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (_, farmId) = SeedUserAndFarm(context);
				var type = SeedAnimalType(context);
				var animal = CreateAnimal(farmId, type.Id, name: "ForSale", sell: 200m);
				context.Animals.Add(animal);
				await context.SaveChangesAsync();
				animalId = animal.Id;
				startBalance = context.Users.First().Balance;
			}

			// Act
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLAnimalRepository(context);
				var sold = await repo.SellAnimalAsync(animalId);
				// Assert
				Assert.NotNull(sold);
				Assert.False(sold!.IsActive);
				Assert.NotNull(sold.SoldAt);
			}

			// Assert (post-conditions)
			await using (var context = new BarnDbContext(options))
			{
				var user = context.Users.First();
				Assert.Equal(startBalance + 200m, user.Balance);
			}
		}

		[Fact]
		public async Task Delete_Should_RemoveAnimal_And_Products_ReturnCount()
		{
			var options = GetInMemoryOptions();
			Guid animalId;
			Guid farmId;
			Guid typeId;

			// Arrange
			await using (var context = new BarnDbContext(options))
			{
				var (_, fId) = SeedUserAndFarm(context);
				farmId = fId;
				var type = SeedAnimalType(context);
				typeId = type.Id;
				var animal = CreateAnimal(farmId, type.Id, name: "Prod");
				context.Animals.Add(animal);
				await context.SaveChangesAsync();
				animalId = animal.Id;

				// add some products
				context.Products.AddRange(
					new Product { Id = Guid.NewGuid(), AnimalId = animalId, Name = "P1", Price = 1m, CreatedAt = DateTime.UtcNow },
					new Product { Id = Guid.NewGuid(), AnimalId = animalId, Name = "P2", Price = 1m, CreatedAt = DateTime.UtcNow }
				);
				await context.SaveChangesAsync();
			}

			// Act & Assert
			await using (var context = new BarnDbContext(options))
			{
				var repo = new SQLAnimalRepository(context);
				var (deleted, productsCount) = await repo.DeleteAnimalAsync(animalId);
				// Assert
				Assert.NotNull(deleted);
				Assert.Equal(2, productsCount);
			}

			// Assert (post-conditions)
			await using (var context = new BarnDbContext(options))
			{
				Assert.False(context.Animals.Any(a => a.Id == animalId));
				Assert.False(context.Products.Any(p => p.AnimalId == animalId));
			}
		}
		
		[Fact]
		public async Task AnimalServices_MarkExpiredAsDeadAsync_ReturnsExpiredIds()
		{
			// Arrange: DI + InMemory Db
			var services = new ServiceCollection();
			services.AddLogging();
			var dbName = Guid.NewGuid().ToString();
			services.AddDbContext<BarnDbContext>(opt => opt.UseInMemoryDatabase(dbName));
			var provider = services.BuildServiceProvider();

			Guid dueId;
			Guid notDueId;
			await using (var scope = provider.CreateAsyncScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
				var (_, farmId) = SeedUserAndFarm(db);
				var type = SeedAnimalType(db, name: "Hen");

				var due = CreateAnimal(farmId, type.Id, name: "Due");
				due.DeathTime = DateTime.UtcNow.AddMinutes(-5); // expired

				var notDue = CreateAnimal(farmId, type.Id, name: "NotDue");
				notDue.DeathTime = DateTime.UtcNow.AddMinutes(30); // not expired

				db.Animals.AddRange(due, notDue);
				await db.SaveChangesAsync();
				dueId = due.Id;
				notDueId = notDue.Id;
			}

			var svc = new AnimalServices(provider);

			// Act
			var ids = await svc.MarkExpiredAsDeadAsync(CancellationToken.None);

			// Assert
			Assert.Contains(dueId, ids);
			Assert.DoesNotContain(notDueId, ids);
		}

	}
}
