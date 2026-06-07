using Moq;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;

namespace ProductivitySystem.UnitTests.Application;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly JwtService _service;

    public JwtServiceTests()
    {
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(c => c["Jwt:Key"]).Returns("super_secret_key_validation_token_1234567890!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("ProductivityBackend");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("ProductivityClients");

        _service = new JwtService(_configMock.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtWithCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Name = "Alex Developer",
            Email = "alex@kpi.ua",
            Role = "Employee"
        };

        // Act
        var tokenString = _service.GenerateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        jwtToken.Issuer.Should().Be("ProductivityBackend");
        jwtToken.Audiences.Should().Contain("ProductivityClients");

        jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be("42");
        jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be("Alex Developer");
        jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be("alex@kpi.ua");
        jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("Employee");
    }
}
