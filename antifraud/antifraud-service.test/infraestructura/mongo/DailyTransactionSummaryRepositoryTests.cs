using antifraud_service.domain.Entities;
using antifraud_service.infrastructure.Mongo;
using MongoDB.Driver;
using Moq;

namespace antifraud_service.tests.Mongo;

public class DailyTransactionSummaryRepositoryTests
{
    private readonly Mock<IMongoCollection<DailyTransactionSummary>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly DailyTransactionSummaryRepository _repository;

    public DailyTransactionSummaryRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<DailyTransactionSummary>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockDatabase
            .Setup(db => db.GetCollection<DailyTransactionSummary>("daily_transaction_summary", null))
            .Returns(_mockCollection.Object);

        _repository = new DailyTransactionSummaryRepository(_mockDatabase.Object);
    }

    [Fact]
    public async Task GetTotalForAsync_ReturnsTotalApprovedValue_WhenSummaryExists()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var date = DateTime.Now;
        var expectedTotal = 100;

        var summary = new DailyTransactionSummary
        {
            Id = $"{sourceAccountId}-{date:yyyyMMdd}",
            SourceAccountId = sourceAccountId,
            Date = date,
            TotalApprovedValue = expectedTotal
        };

        var mockCursor = new Mock<IAsyncCursor<DailyTransactionSummary>>();
        mockCursor.SetupSequence(cursor => cursor.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.Setup(cursor => cursor.Current).Returns(new[] { summary });

        _mockCollection
            .Setup(coll => coll.FindAsync(
                It.IsAny<FilterDefinition<DailyTransactionSummary>>(),

                It.IsAny<FindOptions<DailyTransactionSummary>>(),
                It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var total = await _repository.GetTotalForAsync(sourceAccountId, date);

        // Assert
        Assert.Equal(expectedTotal, total);
    }

    [Fact]
    public async Task GetTotalForAsync_ReturnsZero_WhenSummaryDoesNotExist()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        var mockCursor = new Mock<IAsyncCursor<DailyTransactionSummary>>();
        mockCursor.SetupSequence(cursor => cursor.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(false);

        _mockCollection
            .Setup(coll => coll.FindAsync(
                It.IsAny<FilterDefinition<DailyTransactionSummary>>(),
                It.IsAny<FindOptions<DailyTransactionSummary, DailyTransactionSummary>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var total = await _repository.GetTotalForAsync(sourceAccountId, date);

        // Assert
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task AddToTotalAsync_UpsertsSummaryWithIncrementedValue()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var valueToAdd = 50m;

        var filterDefinition = It.IsAny<FilterDefinition<DailyTransactionSummary>>();
        var updateDefinition = It.IsAny<UpdateDefinition<DailyTransactionSummary>>();

        _mockCollection
            .Setup(coll => coll.UpdateOneAsync(
                filterDefinition,
                updateDefinition,
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

        // Act
        await _repository.AddToTotalAsync(sourceAccountId, date, valueToAdd);

        // Assert
        _mockCollection.Verify(coll => coll.UpdateOneAsync(
            It.IsAny<FilterDefinition<DailyTransactionSummary>>(),
            It.Is<UpdateDefinition<DailyTransactionSummary>>(update => update != null),
            It.Is<UpdateOptions>(options => options.IsUpsert),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
