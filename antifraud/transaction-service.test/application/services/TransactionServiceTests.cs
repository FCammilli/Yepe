using antifraud.domain.Enums;
using Moq;
using transaction_service.application.DTOs;
using transaction_service.application.Services;
using transaction_service.domain.Entities;

namespace transaction_service.application.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ITransactionProducer> _transactionProducerMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionProducerMock = new Mock<ITransactionProducer>();
        _transactionService = new TransactionService(_transactionRepositoryMock.Object, _transactionProducerMock.Object);
    }

    [Fact]
    public async Task Create_ShouldCreateTransactionAndPublishEvent()
    {
        // Arrange
        var createTransactionDTO = new CreateTransactionDTO
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            Value = 100.0m
        };

        _transactionRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<Transaction>())).ReturnsAsync((Transaction transaction) =>
            {
                transaction.Id = Guid.NewGuid();
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.Status = TransactionStatus.Pending;
                return transaction;
            });


        _transactionProducerMock
            .Setup(producer => producer.PublishTransactionCreated(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.Create(createTransactionDTO);

        // Assert
        Assert.NotNull(result);
        _transactionRepositoryMock.Verify(repo => repo.Create(It.IsAny<Transaction>()), Times.Once);
        _transactionProducerMock.Verify(producer => producer.PublishTransactionCreated(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ShouldUpdateTransactionStatus()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var newStatus = TransactionStatus.Approved;

        _transactionRepositoryMock
            .Setup(repo => repo.UpdateStatus(transactionId, newStatus))
            .Returns(Task.CompletedTask);

        // Act
        await _transactionService.UpdateStatus(transactionId, newStatus);

        // Assert
        _transactionRepositoryMock.Verify(repo => repo.UpdateStatus(transactionId, newStatus), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldReturnTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdAt = DateOnly.FromDateTime(DateTime.UtcNow);
        var transaction = new Transaction
        {
            Id = transactionId,
            CreatedAt = createdAt.ToDateTime(TimeOnly.MinValue),
            Value = 100.0m,
            Status = TransactionStatus.Pending,
            TransferTypeId = TransferType.Internal,
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid()
        };

        _transactionRepositoryMock
            .Setup(repo => repo.Get(transactionId, createdAt))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.Get(transactionId, createdAt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
        _transactionRepositoryMock.Verify(repo => repo.Get(transactionId, createdAt), Times.Once);
    }
}
