using antifraud_service.application.Repositories;
using antifraud_service.application.Services;
using Moq;
using SharedKernel.Events;

namespace antifraud_service.application.Tests.Services;

public class AntiFraudServiceTests
{
    private readonly Mock<IDailyTransactionSummaryRepository> _accumulatorMock;
    private readonly Mock<IAntifraudTransactionProducer> _producerMock;
    private readonly AntiFraudService _antiFraudService;

    public AntiFraudServiceTests()
    {
        _accumulatorMock = new Mock<IDailyTransactionSummaryRepository>();
        _producerMock = new Mock<IAntifraudTransactionProducer>();
        _antiFraudService = new AntiFraudService(_accumulatorMock.Object, _producerMock.Object);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldRejectTransaction_WhenValueExceedsLimit()
    {
        // Arrange
        var transaction = new TransactionCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 2500, DateTime.UtcNow);

        // Act
        await _antiFraudService.ProcessTransactionAsync(transaction);

        // Assert
        _producerMock.Verify(p => p.PublishStatusRejectedAsync(transaction.TransactionId, transaction.Value), Times.Once);
        _producerMock.Verify(p => p.PublishStatusApprovedAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldRejectTransaction_WhenAccumulatedValueExceedsLimit()
    {
        // Arrange
        var transaction = new TransactionCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 1500, DateTime.UtcNow);

        _accumulatorMock
            .Setup(a => a.GetTotalForAsync(transaction.SourceAccountId, It.IsAny<DateTime>()))
            .ReturnsAsync(1000);

        // Act
        await _antiFraudService.ProcessTransactionAsync(transaction);

        // Assert
        _producerMock.Verify(p => p.PublishStatusRejectedAsync(transaction.TransactionId, transaction.Value), Times.Once);
        _producerMock.Verify(p => p.PublishStatusApprovedAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldApproveTransaction_WhenValueAndAccumulatedAreWithinLimit()
    {
        // Arrange
        var transaction = new TransactionCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 500, DateTime.UtcNow);

        _accumulatorMock
            .Setup(a => a.GetTotalForAsync(transaction.SourceAccountId, It.IsAny<DateTime>()))
            .ReturnsAsync(1000);

        // Act
        await _antiFraudService.ProcessTransactionAsync(transaction);

        // Assert
        _producerMock.Verify(p => p.PublishStatusApprovedAsync(transaction.TransactionId, transaction.Value), Times.Once);
        _producerMock.Verify(p => p.PublishStatusRejectedAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        _accumulatorMock.Verify(a => a.AddToTotalAsync(transaction.SourceAccountId, It.IsAny<DateTime>(), transaction.Value), Times.Once);
    }
}
