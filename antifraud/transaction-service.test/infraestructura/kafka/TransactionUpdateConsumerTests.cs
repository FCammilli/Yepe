using antifraud.domain.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SharedKernel.Enums;
using SharedKernel.Events;
using transaction_service.application.Services;
using transaction_service.infrastructure.Kafka;

namespace transaction_service.test.infraestructura.kafka;
public class TransactionUpdateConsumerTests
{
    private readonly Mock<IConsumer<string, string>> _mockConsumer;
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Mock<ILogger<TransactionUpdateConsumer>> _mockLogger;
    private readonly Mock<IOptions<KafkaSettings>> _mockKafkaSettings;
    private readonly TransactionUpdateConsumer _transactionUpdateConsumer;

    public TransactionUpdateConsumerTests()
    {
        _mockConsumer = new Mock<IConsumer<string, string>>();
        _mockTransactionService = new Mock<ITransactionService>();
        _mockLogger = new Mock<ILogger<TransactionUpdateConsumer>>();
        _mockKafkaSettings = new Mock<IOptions<KafkaSettings>>();

        _mockKafkaSettings.Setup(x => x.Value).Returns(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group"
        });

        _transactionUpdateConsumer = new TransactionUpdateConsumer(
            _mockTransactionService.Object,
            _mockKafkaSettings.Object,
            _mockLogger.Object
        );

        typeof(TransactionUpdateConsumer)
            .GetField("_consumer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_transactionUpdateConsumer, _mockConsumer.Object);
    }

    [Fact]
    public async Task StartListeningAsync_ShouldProcessValidMessage()
    {
        // Arrange
        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(10);
        var transactionEvent = new TransactionStatusUpdateEvent(
             Guid.NewGuid(),
             100,
            DateTime.UtcNow,
            TransactionStatusEvent.Approved
        );
        var message = JsonConvert.SerializeObject(transactionEvent);

        _mockConsumer
            .Setup(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(new ConsumeResult<string, string>
            {
                Message = new Message<string, string> { Value = message }
            });

        // Act
        var listeningTask = _transactionUpdateConsumer.StartListeningAsync(cancellation.Token);

        await Task.Delay(100);

        // Assert
        _mockTransactionService.Verify(x => x.UpdateStatus(
            transactionEvent.TransactionId,
            TransactionStatus.Approved
        ));

    }

    [Fact]
    public async Task StartListeningAsync_ShouldLogWarningForNullEvent()
    {
        // Arrange
        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(10);
        _mockConsumer
            .Setup(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(new ConsumeResult<string, string>
            {
                Message = new Message<string, string> { Value = null }
            });

        // Act
        var listeningTask = _transactionUpdateConsumer.StartListeningAsync(cancellation.Token);

        // Allow the consumer to process the message
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received null transaction event.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            )
        );

    }

    [Fact]
    public async Task StartListeningAsync_ShouldHandleException()
    {
        // Arrange
        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(10);
        _mockConsumer
            .Setup(x => x.Consume(It.IsAny<TimeSpan>()))
            .Throws(new Exception("Test exception"));

        // Act
        var listeningTask = _transactionUpdateConsumer.StartListeningAsync(cancellation.Token);

        // Allow the consumer to process the message
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing message: Test exception")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            )
        );

    }
}
