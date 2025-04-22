using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrderAccumulator.Exceptions;
using OrderAccumulator.Models;
using OrderAccumulator.Services;
using OrderAccumulator.Settings;
using OrderAccumulator.UnitTests.Builders;

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
    public void GetDefaultMaxExposure_Should_ReturnDefaultValue()
    {
        // Arrange
        const decimal expectedMaxExposure = DefaultMaxExposure;

        // Act
        var result = _exposureService.GetDefaultMaxExposure();

        // Assert
        result.Should().Be(expectedMaxExposure);
    }

    [Fact]
    public void SetDefaultMaxExposure_Should_ChangeTheValue()
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
    public void SetDefaultMaxExposure_Should_ThrowException_When_ValueIsInvalid()
    {
        // Arrange
        const decimal invalidMaxExposure = -1000m;

        // Act
        Action result = () => _exposureService.SetDefaultMaxExposure(invalidMaxExposure);
        
        // Assert
        result.Should().Throw<BusinessValidationException>()
              .WithMessage("Default max exposure must be greater than zero.");
    }
    
    [Theory]
    [InlineData("", 100, 10, OrderSide.Buy)]
    [InlineData("", 200, 9, OrderSide.Sell)]
    public void CalculateExposure_Should_ThrowException_When_SymbolIsNullOrEmpty(string symbol, decimal quantity, decimal price, OrderSide side)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSymbol(symbol)
            .WithQuantity(quantity)
            .WithPrice(price)
            .WithSide(side)
            .Build();
        
        // Act
        var result = () => _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(order);
        
        // Assert
        result.Should().Throw<BusinessValidationException>()
            .WithMessage("Symbol cannot be empty.");
    }
    
    [Theory]
    [InlineData("PETR4", 0, 10, OrderSide.Buy)]
    [InlineData("VALE3", -100, 9, OrderSide.Sell)]
    public void CalculateExposure_Should_ThrowException_When_QuantityIsZeroOrLess(string symbol, decimal quantity, decimal price, OrderSide side)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSymbol(symbol)
            .WithQuantity(quantity)
            .WithPrice(price)
            .WithSide(side)
            .Build();

        // Act
        var result = () => _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(order);
        
        // Assert
        result.Should().Throw<BusinessValidationException>()
            .WithMessage("Quantity must be greater than zero.");
    }
    
    [Theory]
    [InlineData("PETR4", 100, 10, OrderSide.Buy)]
    [InlineData("VALE3", 1000, 8, OrderSide.Buy)]
    public void CalculateExposure_Should_ReturnTrue_AndAddOrder_When_BuyOrderWithinLimit(string symbol, decimal quantity, decimal price, OrderSide side)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSymbol(symbol)
            .WithQuantity(quantity)
            .WithPrice(price)
            .WithSide(side)
            .Build();
        _exposureService.SetDefaultMaxExposure(order.Amount);
        
        // Act
        var result = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(order);

        // Assert
        result.Should().BeTrue();
        _exposureService.GetOrders(symbol).Should().ContainSingle().Which.Should().BeEquivalentTo(order);
    }
    
    [Theory]
    [InlineData("PETR4", 100, 10, OrderSide.Sell)]
    [InlineData("VALE3", 1000, 8, OrderSide.Sell)]
    public void CalculateExposure_Should_ReturnTrue_AndAddOrder_When_SellOrderWithinLimit(string symbol, decimal quantity, decimal price, OrderSide side)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSymbol(symbol)
            .WithQuantity(quantity)
            .WithPrice(price)
            .WithSide(side)
            .Build();
        _exposureService.SetDefaultMaxExposure(order.Amount);
        
        // Act
        var result = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(order);

        // Assert
        result.Should().BeTrue();
        _exposureService.GetOrders(symbol).Should().ContainSingle().Which.Should().BeEquivalentTo(order);
    }

    [Theory]
    [InlineData("PETR4", 100, 10, OrderSide.Buy)]
    [InlineData("VALE3", 1000, 8, OrderSide.Sell)]
    public void CalculateExposure_Should_ReturnFalse_When_OrderExceedsExposureLimit(string symbol, decimal quantity, decimal price, OrderSide side)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSymbol(symbol)
            .WithQuantity(quantity)
            .WithPrice(price)
            .WithSide(side)
            .Build();
        _exposureService.SetDefaultMaxExposure(order.Amount - 1);
        
        // Act
        var result = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(order);

        // Assert
        result.Should().BeFalse();
        _exposureService.GetOrders(symbol).Should().BeEmpty();
    }

    [Fact]
    public void CalculateExposure_Should_AccumulateExposureAcrossOrders_And_HaveOrdersForSymbol()
    {
        // Arrange
        const string symbol = "PETR4";
        var firstOrder = new OrderBuilder().WithSymbol(symbol).WithQuantity(5).WithPrice(10).WithSide(OrderSide.Buy).Build();
        var secondOrder = new OrderBuilder().WithSymbol(symbol).WithQuantity(10).WithPrice(9).WithSide(OrderSide.Buy).Build();
        var thirdOrder = new OrderBuilder().WithSymbol(symbol).WithQuantity(1000).WithPrice(8).WithSide(OrderSide.Buy).Build();
        var expectedExposure = firstOrder.Amount + secondOrder.Amount;
        _exposureService.SetDefaultMaxExposure(expectedExposure);
        
        // Act
        var result1 = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(firstOrder);
        var result2 = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(secondOrder);
        var result3 = _exposureService.CalculateExposureAddOrderIfWithinExposureLimit(thirdOrder);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        
        _exposureService.GetOrders(symbol).Should().HaveCount(2);
        _exposureService.GetExposure(symbol).Should().Be(expectedExposure);
    }
    
    
    
}