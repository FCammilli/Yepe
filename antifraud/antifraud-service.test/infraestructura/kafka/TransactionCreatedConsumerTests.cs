using antifraud_service.application.Services;
using antifraud_service.infrastructure.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SharedKernel.Events;

namespace antifraud_service.test.infraestructura.kafka;

public class TransactionCreatedConsumerTests
{
    private readonly Mock<IAntiFraudService> _antiFraudServiceMock;
    private readonly Mock<IOptions<KafkaSettings>> _kafkaSettingsMock;
    private readonly Mock<ILogger<TransactionCreatedConsumer>> _loggerMock;
    private readonly Mock<IConsumer<string, string>> _consumerMock;
    private readonly TransactionCreatedConsumer _transactionCreatedConsumer;
    public TransactionCreatedConsumerTests()
    {

        _antiFraudServiceMock = new Mock<IAntiFraudService>();
        _kafkaSettingsMock = new Mock<IOptions<KafkaSettings>>();
        _loggerMock = new Mock<ILogger<TransactionCreatedConsumer>>();
        _consumerMock = new Mock<IConsumer<string, string>>();

        _kafkaSettingsMock.Setup(x => x.Value).Returns(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group"
        });
        _transactionCreatedConsumer = new TransactionCreatedConsumer(
           _antiFraudServiceMock.Object,
           _kafkaSettingsMock.Object,
           _loggerMock.Object
       );

        typeof(TransactionCreatedConsumer)
            .GetField("_consumer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_transactionCreatedConsumer, _consumerMock.Object);

    }

    [Fact]
    public async Task StartListeningAsync_ShouldProcessTransactionEvent_WhenMessageIsValid()
    {
        // Arrange
        var transactionEvent = new TransactionCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 100, DateTime.Now);
        var message = JsonConvert.SerializeObject(transactionEvent);

        _consumerMock
            .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(new ConsumeResult<string, string>
            {
                Message = new Message<string, string> { Value = message }
            });

        // Act
        var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(10);
        await _transactionCreatedConsumer.StartListeningAsync(cancellation.Token);

        // Assert
        _antiFraudServiceMock.Verify(x => x.ProcessTransactionAsync(It.IsAny<TransactionCreatedEvent>()));
    }

    [Fact]
    public async Task StartListeningAsync_ShouldLogWarning_WhenMessageIsNull()
    {
        // Arrange
        _consumerMock
            .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(new ConsumeResult<string, string>
            {
                Message = new Message<string, string> { Value = null }
            });

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(10); // Stop after a short delay
        await _transactionCreatedConsumer.StartListeningAsync(cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
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
    public async Task StartListeningAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _consumerMock
            .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Throws(new Exception("Test exception"));

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(100); // Stop after a short delay
        await _transactionCreatedConsumer.StartListeningAsync(cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            )
        );
    }
}
