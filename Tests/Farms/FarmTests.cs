using BarnManagementApi.Controllers;
using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace BarnManagementApi.Tests
{
    public class FarmTests
    {
        private readonly IFarmRepository repository;
        private readonly FarmController farmController;
        private readonly BarnDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<FarmController> logger;

        public FarmTests()
        {
            this.repository = Substitute.For<IFarmRepository>();

            var options = new DbContextOptionsBuilder<BarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            this.context = new BarnDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new Mapping.MappingProfile());
            });
            this.mapper = mapperConfig.CreateMapper();

            this.logger = Substitute.For<ILogger<FarmController>>();

            this.farmController = new FarmController(repository, context, mapper, logger);

        }
        [Fact]
        public async Task Create_Should_Create()
        {
            var farm = new Farm
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Çiftlik",
                Description = "Çok güzel bir yer",
                Location = "Bursa"
            };

            // Arrange mock behavior
            repository.CreateFarmAsync(farm)
                .Returns(farm);

            // Act
            var created = await repository.CreateFarmAsync(farm);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(farm.Id, created.Id);
            Assert.Equal("Çiftlik", created.Name);
            Assert.Equal("Çok güzel bir yer", created.Description);
            Assert.Equal("Bursa", created.Location);
        }

        [Fact]
        public async Task Update_Should_Update()
        {
            // Arrange
            var options = GetInMemoryOptions();

            var id = Guid.NewGuid();
            var existing = new Farm
            {
                Id = id,
                UserId = Guid.NewGuid(),
                Name = "Eski Çiftlik",
                Description = "Eski açıklama",
                Location = "Eski Yer"
            };

            var update = new Farm
            {
                Name = "Yeni Çiftlik",
                Description = "Güncellenmiş açıklama",
                Location = "Yeni Yer"
            };

            // InMemory context
            await using (var context = new BarnDbContext(options))
            {
                context.Farms.Add(existing);
                await context.SaveChangesAsync();
            }

            // Act
            await using (var context = new BarnDbContext(options))
            {
                var repository = new SQLFarmRepository(context);
                await repository.UpdateFarmAsync(id, update);
            }

            // Assert
            await using (var context = new BarnDbContext(options))
            {
                var result = await context.Farms.FindAsync(id);

                Assert.NotNull(result);
                Assert.Equal(id, result.Id);
                Assert.Equal(update.Name, result.Name);
                Assert.Equal(update.Description, result.Description);
                Assert.Equal(update.Location, result.Location);
            }
        }

        [Fact]
        public async Task Delete_Should_Delete()
        {
            var options = GetInMemoryOptions();
            var id = Guid.NewGuid();

            var existing = new Farm
            {
                Id = id,
                UserId = Guid.NewGuid(),
                Name = "Çiftlik",
                Description = "Açıklama",
                Location = "Yer"
            };
            await using (var context = new BarnDbContext(options))
            {
                context.Farms.Add(existing);
                await context.SaveChangesAsync();
            }

            // Act
            await using (var context = new BarnDbContext(options))
            {
                var repository = new SQLFarmRepository(context);
                await repository.DeleteFarmAsync(id);
            }

            // Assert
            await using (var context = new BarnDbContext(options))
            {
                var result = await context.Farms.FindAsync(id);
                Assert.Null(result);
            }

        }


        private static DbContextOptions<BarnDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<BarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}