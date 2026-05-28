using FluentAssertions;
using Xunit;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Auth;
using InvoiceSaaS.Application.Services;
using InvoiceSaaS.Application.Tests.Fixtures;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvoiceSaaS.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IRepository<Company>> _companyRepoMock = new();
    private readonly Mock<IRepository<Role>> _roleRepoMock = new();
    private readonly Mock<IRepository<UserRole>> _userRoleRepoMock = new();

    private AuthService CreateService()
    {
        _uowMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.Repository<Company>()).Returns(_companyRepoMock.Object);
        _uowMock.Setup(u => u.Repository<Role>()).Returns(_roleRepoMock.Object);
        _uowMock.Setup(u => u.Repository<UserRole>()).Returns(_userRoleRepoMock.Object);
        return new AuthService(_uowMock.Object, _jwtMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var user = TestDataBuilder.BuildUser(companyId);
        user.UserRoles = new List<UserRole>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, RoleId = Guid.NewGuid(), Role = new Role { Name = "Admin" } }
        };

        _userRepoMock
            .Setup(r => r.Query())
            .Returns(new List<User> { user }.AsAsyncQueryable());

        _jwtMock.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>())).Returns("access_token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
        _jwtMock.Setup(j => j.GetRefreshTokenExpiry()).Returns(DateTime.UtcNow.AddDays(7));

        var service = CreateService();
        var request = new LoginRequest { Email = user.Email, Password = "Admin123!" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.Query())
            .Returns(new List<User>().AsAsyncQueryable());

        var service = CreateService();
        var request = new LoginRequest { Email = "nonexistent@test.com", Password = "password" };

        // Act
        var act = async () => await service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var user = TestDataBuilder.BuildUser(companyId);
        user.UserRoles = new List<UserRole>();

        _userRepoMock
            .Setup(r => r.Query())
            .Returns(new List<User> { user }.AsAsyncQueryable());

        var service = CreateService();
        var request = new LoginRequest { Email = user.Email, Password = "WrongPassword" };

        // Act
        var act = async () => await service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_LockedAccount_ThrowsBusinessException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var user = TestDataBuilder.BuildUser(companyId);
        user.LockedUntil = DateTime.UtcNow.AddMinutes(10);
        user.UserRoles = new List<UserRole>();

        _userRepoMock
            .Setup(r => r.Query())
            .Returns(new List<User> { user }.AsAsyncQueryable());

        var service = CreateService();
        var request = new LoginRequest { Email = user.Email, Password = "Admin123!" };

        // Act
        var act = async () => await service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*");
    }

    [Fact]
    public async Task LogoutAsync_ValidUser_ClearsRefreshToken()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var user = TestDataBuilder.BuildUser(companyId);
        user.RefreshToken = "some_token";

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, default))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        await service.LogoutAsync(user.Id);

        // Assert
        user.RefreshToken.Should().BeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
