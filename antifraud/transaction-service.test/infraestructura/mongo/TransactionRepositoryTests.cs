using antifraud.domain.Enums;
using MongoDB.Driver;
using Moq;
using transaction_service.domain.Entities;
using transaction_service.infrastructure.Mongo;

namespace transaction_service.tests.mongo;

public class TransactionRepositoryTests
{
    private readonly Mock<IMongoCollection<Transaction>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Transaction>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockDatabase
            .Setup(db => db.GetCollection<Transaction>("transactions", null))
            .Returns(_mockCollection.Object);

        _repository = new TransactionRepository(_mockDatabase.Object);
    }

    [Fact]
    public async Task Create_ShouldInsertTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Pending,
            Value = 100.0m
        };

        // Act
        var result = await _repository.Create(transaction);

        // Assert
        _mockCollection.Verify(
            c => c.InsertOneAsync(transaction, null, default),
            Times.Once
        );
        Assert.Equal(transaction, result);
    }


    [Fact]
    public async Task UpdateStatus_ShouldUpdateTransactionStatus()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var createdAt = DateOnly.FromDateTime(DateTime.UtcNow);
        var transaction = new Transaction
        {
            Id = transactionId,
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            CreatedAt = createdAt.ToDateTime(new TimeOnly(12, 0)),
            Status = TransactionStatus.Pending,
            Value = 100.0m
        };

        var mockCursor = new Mock<IAsyncCursor<Transaction>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.Setup(c => c.Current).Returns(new[] { transaction });
        _mockCollection
          .Setup(c => c.FindAsync(
              It.IsAny<FilterDefinition<Transaction>>(),
              It.IsAny<FindOptions<Transaction>>(),
              It.IsAny<CancellationToken>()
          ))
          .ReturnsAsync(mockCursor.Object);


        // Act
        var result = await _repository.Get(transactionId, createdAt);

        // Assert
        Assert.Equal(transaction, result);
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
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = TransferType.Internal,
            CreatedAt = createdAt.ToDateTime(new TimeOnly(12, 0)),
            Status = TransactionStatus.Pending,
            Value = 100.0m
        };

        var mockCursor = new Mock<IAsyncCursor<Transaction>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<System.Threading.CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.Setup(c => c.Current).Returns(new[] { transaction });

        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Transaction>>(),
                It.IsAny<FindOptions<Transaction>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _repository.Get(transactionId, createdAt);

        // Assert
        Assert.Equal(transaction, result);
    }
}
