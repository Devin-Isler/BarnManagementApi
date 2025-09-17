using AutoMapper;
using BarnManagementApi.Controllers;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace BarnManagementApi.Tests
{
	public class UserTests
	{
		private static ClaimsPrincipal CreateUserPrincipal(Guid userId)
		{
			return new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, userId.ToString())
			}, "TestAuth"));
		}

		private static IMapper CreateMapper()
		{
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new Mapping.MappingProfile());
			});
			return mapperConfig.CreateMapper();
		}

		private static UserController CreateController(
			IUserRepository userRepository,
			IMapper mapper,
			UserManager<IdentityUser> userManager,
			ILogger<UserController> logger,
			ITokenRepository tokenRepository,
			Guid currentUserId)
		{
			var controller = new UserController(userRepository, mapper, userManager, logger, tokenRepository)
			{
				ControllerContext = new ControllerContext
				{
					HttpContext = new DefaultHttpContext
					{
						User = CreateUserPrincipal(currentUserId)
					}
				}
			};
			return controller;
		}

		private static UserManager<IdentityUser> CreateUserManagerSubstitute()
		{
			var store = Substitute.For<IUserStore<IdentityUser>>();
			var userManager = Substitute.For<UserManager<IdentityUser>>(store, null, null, null, null, null, null, null, null);
			return userManager;
		}

		[Fact]
		public async Task GetMe_Should_ReturnOk_WithUserDto_When_UserExists()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var domainUser = new User
			{
				Id = userId,
				Username = "user@example.com",
				Balance = 100,
				CreatedAt = DateTime.UtcNow
			};

			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns(domainUser);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);

			// Act
			var result = await controller.GetMe();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<UserDto>(ok.Value);
			Assert.Equal(domainUser.Id, dto.Id);
			Assert.Equal(domainUser.Username, dto.Username);
			Assert.Equal(domainUser.Balance, dto.Balance);
			await repo.Received(1).GetByIdAsync(userId);
		}

		[Fact]
		public async Task GetMe_Should_ReturnNotFound_When_UserMissing()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns((User?)null);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);

			// Act
			var result = await controller.GetMe();

			// Assert
			Assert.IsType<NotFoundResult>(result);
			await repo.Received(1).GetByIdAsync(userId);
		}

		[Fact]
		public async Task UpdateMe_Should_UpdateUsername_And_ReturnOk()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var existing = new User
			{
				Id = userId,
				Username = "old@example.com",
				PasswordHash = "hash",
				Balance = 10,
				CreatedAt = DateTime.UtcNow
			};

			var updated = new User
			{
				Id = userId,
				Username = "new@example.com",
				PasswordHash = existing.PasswordHash,
				Balance = 10,
				CreatedAt = existing.CreatedAt,
				UpdatedAt = DateTime.UtcNow
			};

			var request = new UserUpdateDto { Username = "new@example.com", Password = null };

			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns(existing);
			repo.UpdateAsync(Arg.Any<User>()).Returns(updated);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();

			var userManager = CreateUserManagerSubstitute();
			var identityUser = new IdentityUser { Id = userId.ToString(), UserName = existing.Username, Email = existing.Username };
			userManager.FindByIdAsync(userId.ToString()).Returns(identityUser);
			userManager.UpdateAsync(identityUser).Returns(IdentityResult.Success);

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);

			// Act
			var result = await controller.UpdateMe(request);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<UserDto>(ok.Value);
			Assert.Equal(updated.Username, dto.Username);
			await userManager.Received(1).UpdateAsync(Arg.Any<IdentityUser>());
			await repo.Received(1).UpdateAsync(Arg.Is<User>(u => u.Id == userId && u.Username == "new@example.com"));
		}

		[Fact]
		public async Task UpdateMe_Should_ReturnNotFound_When_IdentityUserMissing()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var existing = new User { Id = userId, Username = "old@example.com", PasswordHash = "hash" };

			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns(existing);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();
			userManager.FindByIdAsync(userId.ToString()).Returns((IdentityUser?)null);

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);
			var request = new UserUpdateDto { Username = "new@example.com", Password = null };

			// Act
			var result = await controller.UpdateMe(request);

			// Assert
			var notFound = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal("Identity user not found.", notFound.Value);
		}

		[Fact]
		public async Task SetBalance_Should_ReturnNotFound_When_UserMissing()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var repo = Substitute.For<IUserRepository>();
			repo.SetBalanceAsync(userId, 100).Returns((User?)null);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);
			var req = new AdjustBalanceDto { Amount = 100 };

			// Act
			var result = await controller.SetBalance(req);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task SetBalance_Should_UpdateBalance_And_ReturnOk()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var initial = new User { Id = userId, Username = "user@example.com", Balance = 50, CreatedAt = DateTime.UtcNow };
			var updated = new User { Id = userId, Username = initial.Username, Balance = 200, CreatedAt = initial.CreatedAt, UpdatedAt = DateTime.UtcNow };

			var repo = Substitute.For<IUserRepository>();
			repo.SetBalanceAsync(userId, 200).Returns(updated);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);
			var req = new AdjustBalanceDto { Amount = 200 };

			// Act
			var result = await controller.SetBalance(req);

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<UserDto>(ok.Value);
			Assert.Equal(updated.Id, dto.Id);
			Assert.Equal(updated.Username, dto.Username);
			Assert.Equal(200, dto.Balance);
			await repo.Received(1).SetBalanceAsync(userId, 200);
		}

		[Fact]
		public async Task DeleteUser_Should_DeleteAndReturnOk()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var existing = new User { Id = userId, Username = "user@example.com", Balance = 0 };

			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns(existing);
			repo.DeleteUserAsync(userId).Returns((true, 1, 2, 3));

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();

			var userManager = CreateUserManagerSubstitute();
			var identityUser = new IdentityUser { Id = userId.ToString(), UserName = existing.Username, Email = existing.Username };
			userManager.FindByIdAsync(userId.ToString()).Returns(identityUser);
			userManager.DeleteAsync(identityUser).Returns(IdentityResult.Success);

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);

			// Act
			var result = await controller.DeleteUser();

			// Assert
			var ok = Assert.IsType<OkObjectResult>(result);
			var dto = Assert.IsType<UserDto>(ok.Value);
			Assert.Equal(existing.Id, dto.Id);
			tokenRepo.Received(1).BlacklistUserTokens(userId.ToString());
			await userManager.Received(1).DeleteAsync(identityUser);
			await repo.Received(1).DeleteUserAsync(userId);
		}

		[Fact]
		public async Task DeleteUser_Should_ReturnNotFound_When_UserMissing()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var repo = Substitute.For<IUserRepository>();
			repo.GetByIdAsync(userId).Returns((User?)null);

			var mapper = CreateMapper();
			var logger = Substitute.For<ILogger<UserController>>();
			var tokenRepo = Substitute.For<ITokenRepository>();
			var userManager = CreateUserManagerSubstitute();

			var controller = CreateController(repo, mapper, userManager, logger, tokenRepo, userId);

			// Act
			var result = await controller.DeleteUser();

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}
	}
}
