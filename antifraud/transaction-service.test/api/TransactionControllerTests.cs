using antifraud.domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using transaction_service.api.Controllers;
using transaction_service.application.DTOs;
using transaction_service.application.Services;
using transaction_service.domain.Entities;

namespace transaction_service.api.Tests.controllers;

public class TransactionControllerTests
{
    private readonly Mock<ILogger<TransactionController>> _mockLogger;
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly TransactionController _controller;

    public TransactionControllerTests()
    {
        _mockLogger = new Mock<ILogger<TransactionController>>();
        _mockTransactionService = new Mock<ITransactionService>();
        _controller = new TransactionController(_mockLogger.Object, _mockTransactionService.Object);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsCreatedResult_WhenSuccessful()
    {
        // Arrange
        var createTransactionDTO = new CreateTransactionDTO
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            Value = 100
        };
        var expectedResult = "mock-guid";

        _mockTransactionService
            .Setup(service => service.Create(createTransactionDTO))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Transaction(createTransactionDTO);

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result);
        var createdResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(expectedResult, createdResult.Value);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenServiceReturnsNull()
    {
        // Arrange
        var createTransactionDTO = new CreateTransactionDTO
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            Value = 100
        };

        _mockTransactionService
            .Setup(service => service.Create(createTransactionDTO))
            .ReturnsAsync((string)null);

        // Act
        var result = await _controller.Transaction(createTransactionDTO);

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result);
        Assert.IsType<BadRequestResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetTransaction_ReturnsOkResult_WhenTransactionFound()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var transaction = new Transaction
        {
            Id = transactionId,
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Approved,
            Value = 100
        };

        _mockTransactionService
            .Setup(service => service.Get(transactionId, createdDate))
            .ReturnsAsync(transaction);

        // Act
        var result = await _controller.Transaction(transactionId, createdDate);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Transaction>>(result);
        var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(transaction, okResult.Value);
    }

    [Fact]
    public async Task GetTransaction_ReturnsNotFound_WhenTransactionNotFound()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockTransactionService
            .Setup(service => service.Get(transactionId, createdDate))
            .ReturnsAsync((Transaction)null);

        // Act
        var result = await _controller.Transaction(transactionId, createdDate);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Transaction>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetTransaction_ReturnsBadRequest_WhenParametersAreInvalid()
    {
        // Act
        var result = await _controller.Transaction(Guid.Empty, null);
        var expectedResult = new { error = "The date and id parameters are required." };
        // Assert
        var actionResult = Assert.IsType<ActionResult<Transaction>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equivalent(expectedResult, badRequestResult.Value);
    }
}
