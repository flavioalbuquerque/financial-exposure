using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrderAccumulator.Exceptions;
using OrderAccumulator.Services;
using OrderAccumulator.Settings;

namespace OrderAccumulator.UnitTests.Services;

public class ExposureServiceTests
{
    private const decimal DefaultMaxExposure = 100000000m;

    private readonly ExposureService _exposureService;

    public ExposureServiceTests()
    {
        var mockLogger = new Mock<ILogger<ExposureService>>();
        var mockExposureSettings = new Mock<IOptions<ExposureSettings>>();
        
        mockExposureSettings.Setup(config => 
            config.Value).Returns(new ExposureSettings { DefaultMaxExposure = DefaultMaxExposure });
        
        _exposureService = new ExposureService(mockLogger.Object, mockExposureSettings.Object);
    }

    [Fact]
    public void GetDefaultMaxExposure_ShouldReturnDefaultValue()
    {
        // Arrange
        const decimal expectedMaxExposure = DefaultMaxExposure;

        // Act
        var result = _exposureService.GetDefaultMaxExposure();

        // Assert
        result.Should().Be(expectedMaxExposure);
    }

    [Fact]
    public void SetDefaultMaxExposure_ShouldChangeTheValue()
    {
        // Arrange
        const decimal newMaxExposure = 6000m;

        // Act
        _exposureService.SetDefaultMaxExposure(newMaxExposure);

        // Assert
        var result = _exposureService.GetDefaultMaxExposure();
        result.Should().Be(newMaxExposure);
    }
    
    [Fact]
    public void SetDefaultMaxExposure_ShouldThrowException_WhenValueIsInvalid()
    {
        // Arrange
        const decimal invalidMaxExposure = -1000m;

        // Act
        Action act = () => _exposureService.SetDefaultMaxExposure(invalidMaxExposure);
        
        // Assert
        act.Should().Throw<BusinessValidationException>()
            .WithMessage("Default max exposure must be greater than zero.");
    }

}